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
/

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
/

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
/