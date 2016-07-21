CREATE DEFINER=`root`@`localhost` PROCEDURE `GetSpinResult`(IN playerIdVal INT UNSIGNED,
								  IN coinsBet INT UNSIGNED,
                                  IN coinsWon INT UNSIGNED,
								  IN hashValue VARCHAR(256),
                                  OUT pName VARCHAR(60),
                                  OUT pCredits INT UNSIGNED,
                                  OUT pLifetimeSpins INT UNSIGNED,
                                  OUT pLifetimeAvgReturn DECIMAL(11,3)
                                  )
BEGIN
    DECLARE coinDelta INT;
    DECLARE pSaltValue VARCHAR(20);
    DECLARE selectCheck INT;
    DECLARE updateCheck INT;  
    DECLARE hashCheckValue VARCHAR(100);
    
    DECLARE invalid_value CONDITION FOR SQLSTATE '45000';
    
    DECLARE EXIT HANDLER FOR invalid_value 
		 IF playerIdVal = 0  THEN 
		   RESIGNAL SET MESSAGE_TEXT = 'invalid playerIdVal value.';
		 ELSEIF coinsBet = 0 THEN 
		   RESIGNAL SET MESSAGE_TEXT = 'invalid coinsBet value.';
		 ELSEIF selectCheck != 1 THEN 
		   RESIGNAL SET MESSAGE_TEXT = 'playerId not found in playerdata table.';
		 ELSEIF updateCheck != 1 THEN 
		   RESIGNAL SET MESSAGE_TEXT = 'playerId not found in playerdata table.';
		 ELSEIF hashValue IS NULL OR hashValue = "" THEN 
		   RESIGNAL SET MESSAGE_TEXT = 'invalid hashValue value.';
		 ELSEIF hashValue != hashCheckValue THEN 
		   RESIGNAL SET MESSAGE_TEXT = 'invalid hashValue value.';
		 END IF;
     
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
		SELECT 'An error has occurred and the stored procedure was terminated';
	END;

	# Initialize output values
	SET pName = "";
    SET pCredits = 0;
    SET pLifetimeSpins = 0;
	SET pLifetimeAvgReturn = 0.0;
    
    # validate input parameter values
	IF ( playerIdVal = 0 OR hashValue IS NULL OR hashValue = "" OR coinsBet = 0) THEN
		SIGNAL invalid_value;
	END IF;

	# get the players current statistics from the playerdata table
	SELECT PlayerName, Credits, LifetimeSpins, SaltValue
	INTO pName, pCredits, pLifetimeSpins, pSaltValue
	FROM playerdata
	WHERE PlayerID = playerIdVal;
    
    SET selectCheck = 0;

	SET selectCheck = (SELECT FOUND_ROWS()); 
	# error if no row was found for the player
    IF ( selectCheck != 1 ) THEN
		SIGNAL invalid_value;
	END IF;
 
	# check the hash value sent by the caller
    SET hashCheckValue = CONCAT(pSaltValue,playerIdVal);
    SET hashCheckValue = SHA2(hashCheckValue, 256);
    
    IF ( hashValue != hashCheckValue )THEN
		SIGNAL invalid_value;
	END IF;

	#compute the players coint delta
	SET coinDelta = coinsWon - coinsBet;
   
    IF ( (pCredits + coinDelta) < 0 ) THEN
		SIGNAL SQLSTATE '45000'
		SET MESSAGE_TEXT = 'not enough player credits for this play.';
	END IF;

	#update the player LifetimeSpins and Credits values
	UPDATE playerdata
		SET LifetimeSpins=LifetimeSpins + 1,
			Credits = Credits + coinDelta
	WHERE PlayerID = playerIdVal
    LIMIT 1;
    
    SET updateCheck = 0;

    SET updateCheck = (SELECT ROW_COUNT());
	# error if no row was updated
    IF ( updateCheck != 1 ) THEN
		SIGNAL invalid_value;
	END IF;
    
    # compute the player's lifetime average return
	SET pLifetimeAvgReturn = CAST(pLifetimeSpins/pCredits as DECIMAL(11,3));
END