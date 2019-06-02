--------------------------------------------------
--------------FUNC_ADD_LIKE--------------------------------//
CREATE OR REPLACE 
FUNCTION FUNC_ADD_LIKE 
(user_id IN INTEGER, message_id IN INTEGER)
RETURN INTEGER
AS
temp_date DATE;
state INTEGER:=1;
BEGIN

  SELECT count(*) into state 
  FROM MESSAGE
  WHERE MESSAGE.MESSAGE_ID=message_id;

  if state=0
  THEN
    return state;
  END IF;

  SELECT count(*) into state 
  FROM MESSAGE_OWNS_TOPIC
  WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=message_id;

  if state=0
  THEN
    return state;
  END IF;



  UPDATE TOPIC temp_topic
  SET TOPIC_HEAT=TOPIC_HEAT+1
  WHERE TEMP_TOPIC.TOPIC_ID IN (SELECT TOPIC_ID
                                FROM MESSAGE_OWNS_TOPIC
                                WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=message_id);

  UPDATE MESSAGE
  set MESSAGE_AGREE_NUM=MESSAGE_AGREE_NUM+1,MESSAGE_HEAT=MESSAGE_HEAT+1
  WHERE MESSAGE.MESSAGE_ID=message_id;




  SELECT SYSDATE into temp_date from dual ;

  insert into LIKES
      (LIKES_ID,LIKES_USER_ID, LIKES_MESSAGE_ID,LIKES_TIME)
  values(seq_likes.nextval,user_id, message_id, temp_date);
  state:=1;

	RETURN state;
END;

--------------------------------------------------
--------------FUNC_ADD_PRIVATE_LETTER--------------------------------//
CREATE OR REPLACE 
FUNCTION "FUNC_ADD_PRIVATE_LETTER" (sender_user_id IN INTEGER, receiver_user_id IN INTEGER, content IN VARCHAR2)
RETURN INTEGER
AS
temp_date DATE;
state integer:=1;
BEGIN
  select sysdate  into temp_date from dual ;

  insert into PRIVATE_LETTER
      (PRIVATE_LETTER_ID,PRIVATE_LETTER_CONTENT, PRIVATE_LETTER_IS_READ,PRIVATE_LETTER_CREATE_TIME,PRIVATE_LETTER_SENDER_ID,PRIVATE_LETTER_RECEIVER_ID)
  values(seq_private_letter.nextval, content, '0', temp_date, sender_user_id, receiver_user_id);

	RETURN state;
END;

--------------------------------------------------
--------------FUNC_DELETE_LIKE--------------------------------//


CREATE OR REPLACE 
FUNCTION FUNC_DELETE_LIKE
(user_id IN INTEGER, message_id IN INTEGER)
RETURN INTEGER
AS
state INTEGER:=1;
BEGIN

	SELECT count(*) into state 
  FROM MESSAGE
  WHERE MESSAGE.MESSAGE_ID=message_id;

  if state=0
  THEN
    return state;
  END IF;

  SELECT count(*) into state 
  FROM MESSAGE_OWNS_TOPIC
  WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=message_id;

  if state=0
  THEN
    return state;
  END IF;

  UPDATE TOPIC temp_topic
  SET TOPIC_HEAT=TOPIC_HEAT-1
  WHERE TEMP_TOPIC.TOPIC_ID IN (SELECT TOPIC_ID
                                FROM MESSAGE_OWNS_TOPIC
                                WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=message_id);

  UPDATE MESSAGE
  set MESSAGE_AGREE_NUM=MESSAGE_AGREE_NUM+1,MESSAGE_HEAT=MESSAGE_HEAT-1
  WHERE MESSAGE.MESSAGE_ID=message_id;

  DELETE from LIKES
  where LIKES.LIKES_USER_ID=user_id and LIKES.LIKES_MESSAGE_ID=message_id;
  state:=1;

	RETURN state;
END;


--------------------------------------------------
--------------FUNC_DELETE_PRIVATE_LETTER--------------------------------//
CREATE OR REPLACE 
FUNCTION "FUNC_DELETE_PRIVATE_LETTER" (private_letter_id IN INTEGER)
RETURN INTEGER
AS
state integer:=1;
BEGIN
  select count(*) into state 
  from PRIVATE_LETTER
  where private_letter_id=PRIVATE_LETTER.PRIVATE_LETTER_ID;
if state=0
  then 
  return state;
ELSE
  DELETE from PRIVATE_LETTER
  where PRIVATE_LETTER.PRIVATE_LETTER_ID=private_letter_id;
  state:=1;
end if;
return state;
END;


--------------------------------------------------
--------------FUNC_GET_USER_AVATAR--------------------------------//
CREATE OR REPLACE 
function 
FUNC_GET_USER_AVATAR(user_id in INTEGER, avatar_id out INTEGER)
return INTEGER
is 
state INTEGER;
begin 

  select count(*) into state 
  from Avatar_Image
  where user_id=Avatar_Image.user_id and avatar_image_in_use=1;

if state>0
  then 
  state:=1;
  select 
  avatar_image_id into avatar_id 
  from Avatar_Image
  where user_id=Avatar_Image.user_id and avatar_image_in_use=1;
  return state;
end if;
return state;
end;


--------------------------------------------------
--------------FUNC_QUERY_MESSAGE_IDS_LIKES--------------------------------//
CREATE OR REPLACE 
FUNCTION FUNC_QUERY_MESSAGE_IDS_LIKES
(user_id IN INTEGER, startFrom IN INTEGER, limitation IN INTEGER, search_result OUT Sys_refcursor)
RETURN INTEGER
AS
state INTEGER:=1;

BEGIN

	SELECT count(*) into state 
  from LIKES
  WHERE LIKES.LIKES_USER_ID=user_id;

  IF state=0
  THEN 
    return state;
  ELSE  
    open search_result for SELECT* FROM 
         (SELECT LIKES_MESSAGE_ID
          FROM LIKES
          WHERE LIKES.LIKES_ID>=startFrom
              AND LIKES.LIKES_USER_ID=user_id
         ORDER BY LIKES.LIKES_TIME DESC)
    WHERE ROWNUM<=limitation;

    state:=1;
  END IF;
	RETURN state;

END;


--------------------------------------------------
--------------FUNC_QUERY_PRIVATE_LETTERS--------------------------------//

CREATE OR REPLACE 
FUNCTION FUNC_QUERY_PRIVATE_LETTERS
(user_id IN INTEGER, startFrom IN INTEGER, limitation IN INTEGER,search_result OUT sys_refcursor)
RETURN INTEGER
AS
state integer:=1;

BEGIN

  SELECT count(*) into state 
  from PRIVATE_LETTER
  where PRIVATE_LETTER.PRIVATE_LETTER_RECEIVER_ID=user_id;

  IF state=0
  THEN 
    return state;
  ELSE  
    open search_result for SELECT* FROM 
         (SELECT PRIVATE_LETTER_SENDER_ID,PRIVATE_LETTER_ID,PRIVATE_LETTER_CONTENT,PRIVATE_LETTER_CREATE_TIME
         from PRIVATE_LETTER
         WHERE PRIVATE_LETTER.PRIVATE_LETTER_RECEIVER_ID=user_id 
            and PRIVATE_LETTER.PRIVATE_LETTER_ID>=startFrom
         ORDER BY PRIVATE_LETTER.PRIVATE_LETTER_CREATE_TIME DESC)
    WHERE ROWNUM<=limitation;

    state:=1;
  END IF;
	RETURN state;
END;


--------------------------------------------------
--------------FUNC_QUERY_TOPIC_IDS_BY_HEAT--------------------------------//

CREATE OR REPLACE 
FUNCTION FUNC_QUERY_TOPIC_IDS_BY_HEAT
(startFrom IN INTEGER, limitation IN INTEGER, search_result OUT sys_refcursor)
RETURN INTEGER
AS
state INTEGER:=1;

BEGIN

	SELECT count(*) into state 
  from TOPIC;

  IF state=0
  THEN 
    return state;
  ELSE  
    open search_result for SELECT* FROM 
         (SELECT TOPIC_ID
          FROM TOPIC
          WHERE TOPIC.TOPIC_ID>=startFrom
         ORDER BY TOPIC.TOPIC_HEAT DESC)
    WHERE ROWNUM<=limitation;

    state:=1;
  END IF;
	RETURN state;
END;
