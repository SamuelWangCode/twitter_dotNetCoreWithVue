---------------------------------------------------
--------------FUNC_ADD_LIKE--------------------------------//
CREATE OR REPLACE 
FUNCTION FUNC_ADD_LIKE 
(user_id IN INTEGER, like_message_id IN INTEGER)
RETURN INTEGER
AS
temp_date VARCHAR2(30);
temp_topic_state INTEGER:=0;
state INTEGER:=1;
BEGIN

  SELECT count(*) into state 
  FROM MESSAGE
  WHERE MESSAGE.MESSAGE_ID=like_message_id;

  if state=0
  THEN
    return state;
  END IF;

  SELECT count(*) into temp_topic_state 
  FROM MESSAGE_OWNS_TOPIC
  WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=like_message_id;

  if temp_topic_state>0
  THEN
    UPDATE TOPIC temp_topic
    SET TOPIC_HEAT=TOPIC_HEAT+1
    WHERE TEMP_TOPIC.TOPIC_ID IN (SELECT TOPIC_ID
                                  FROM MESSAGE_OWNS_TOPIC
                                  WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=like_message_id);
  END IF;



  UPDATE MESSAGE
  set MESSAGE_AGREE_NUM=MESSAGE_AGREE_NUM+1,MESSAGE_HEAT=MESSAGE_HEAT+1
  WHERE MESSAGE.MESSAGE_ID=like_message_id;




  SELECT to_char(sysdate,'yyyy-mm-dd HH24:MI:SS')into temp_date from dual ;

  insert into LIKES
      (LIKES_USER_ID, LIKES_MESSAGE_ID,LIKES_TIME)
  values(user_id, like_message_id, temp_date);
  state:=1;

	RETURN state;
END;

/

--------------------------------------------------
--------------FUNC_DELETE_LIKE--------------------------------//


CREATE OR REPLACE 
FUNCTION FUNC_DELETE_LIKE
(user_id IN INTEGER, like_message_id IN INTEGER)
RETURN INTEGER
AS
state INTEGER:=1;
topic_state INTEGER:=1;
BEGIN

	SELECT count(*) into state 
  FROM MESSAGE
  WHERE MESSAGE.MESSAGE_ID=like_message_id;

  if state=0
  THEN
    return state;
  END IF;

  SELECT count(*) into topic_state 
  FROM MESSAGE_OWNS_TOPIC
  WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=like_message_id;

  if topic_state=0
  THEN
    UPDATE TOPIC temp_topic
    SET TOPIC_HEAT=TOPIC_HEAT-1
    WHERE TEMP_TOPIC.TOPIC_ID IN (SELECT TOPIC_ID
                                  FROM MESSAGE_OWNS_TOPIC
                                  WHERE MESSAGE_OWNS_TOPIC.MESSAGE_ID=like_message_id);
  END IF;

  

  UPDATE MESSAGE
  set MESSAGE_AGREE_NUM=MESSAGE_AGREE_NUM-1,MESSAGE_HEAT=MESSAGE_HEAT-1
  WHERE MESSAGE.MESSAGE_ID=like_message_id;

  DELETE from LIKES
  where LIKES.LIKES_USER_ID=user_id and LIKES.LIKES_MESSAGE_ID=like_message_id;
  state:=1;

	RETURN state;
END;

/





--------------------------------------------------
--------------FUNC_QUERY_MESSAGE_IDS_LIKES--------------------------------//
create or replace FUNCTION FUNC_QUERY_MESSAGE_IDS_LIKES
(user_id IN INTEGER, startFrom IN INTEGER, limitation IN INTEGER, search_result OUT Sys_refcursor)
RETURN INTEGER
AS
state INTEGER:=1;

BEGIN
open search_result for 
      SELECT* FROM 
        (SELECT LIKES_MESSAGE_ID
          FROM LIKES
          WHERE LIKES.LIKES_USER_ID=user_id
          ORDER BY LIKES.LIKES_TIME DESC)
          WHERE ROWNUM<startFrom+limitation
        MINUS
      SELECT* FROM 
         (SELECT LIKES_MESSAGE_ID
          FROM LIKES
          WHERE LIKES.LIKES_USER_ID=user_id
         ORDER BY LIKES.LIKES_TIME DESC)
    WHERE ROWNUM<startFrom;

state:=1;
RETURN state;

END;
/

--------------------------------------------------
--------------FUNC_QUERY_USER_LIKES_MESSAGE--------------------------------//
create or replace FUNCTION FUNC_QUERY_USER_LIKES_MESSAGE
(user_id IN INTEGER, message_id IN INTEGER)
RETURN INTEGER
AS
state INTEGER:=1;

BEGIN
select count(*) into state
from LIKES where LIKES_USER_ID = user_id and LIKES_MESSAGE_ID = message_id;
RETURN state;

END;
/