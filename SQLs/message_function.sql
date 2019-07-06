---------------FUNC_SHOW_ MESSAGE_BY_ID----------------------
create or replace function 
FUNC_SHOW_MESSAGE_BY_ID(message_id_input in INTEGER, result out sys_refcursor)
return INTEGER
is
state INTEGER:=0;
begin
select count(*) into state 
from message
where message_id=message_id_input;
if state != 0 then
open result for
select *
from message natural left join message_image natural left join transpond
where message_id =message_id_input;
end if;


return state;
end;
/


---------------FUNC_SHOW_ MESSAGE_BY_RANGE----------------------
-----------------------??????????-------------------------------
create or replace function 
FUNC_SHOW_MESSAGE_BY_RANGE(user_id in INTEGER, rangeStart in INTEGER, rangeLimitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
state INTEGER:=0;

begin

select count(*) into state
from MESSAGE 
where MESSAGE_SENDER_USER_ID = user_id;

if state!=0 then
  open search_result for 
  select * from 
    (select * 
     from (message natural left join message_image) natural left join transpond
     where message_sender_user_id=user_id
     order by message_id desc)
  where rownum <rangeLimitation+rangeStart
  minus
  select * from 
    (select * 
     from (message natural left join message_image) natural left join transpond
     where message_sender_user_id=user_id
     order by message_id desc)
  where rownum <rangeStart;

end if;
return state;
end;
/





---------------------FUNC_SEND_MESSAGE-------------------------------
---------------------????????????Message?---------------
create or replace function
FUNC_SEND_MESSAGE(message_content in VARCHAR2, message_has_image in INTEGER, user_id in INTEGER, message_image_count in INTEGER, message_id out INTEGER)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
create_time VARCHAR2(30);
temp_id INTEGER:=0;
begin
select to_char(sysdate, 'yyyy-mm-dd HH24:MI:SS') into create_time from dual;
insert into Message(message_content, message_create_time, message_agree_num, message_transponded_num,
					message_comment_num, message_view_num, message_has_image, message_sender_user_id, message_heat)
values(message_content, create_time, '0', '0', '0', '0', message_has_image, user_id, '0');

select message_id into temp_id 
from Message
where message_create_time=create_time;

message_id:=temp_id;

if message_has_image !=0 then
insert into Message_Image(message_id, message_image_count)
values (temp_id, message_image_count);
end if;

commit;
return state;
end;
/



-------------------FUNC_TRANSPOND_MESSAGE--------------------
-------------------转发1条推特（Message和Transpond添加�??
create or replace function
FUNC_TRANSPOND_MESSAGE(message_content in VARCHAR2, message_source_is_transpond in INTEGER, message_sender_user_id in INTEGER, message_transpond_message_id in INTEGER, message_id out INTEGER)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
transpond_time VARCHAR2(30);
out_id INTEGER :=0;
transpond_used_id INTEGER:=0;
message_has_image INTEGER:=0;
message_image_count INTEGER:=0;
begin
select to_char(sysdate,'yyyy-mm-dd HH24:MI:SS')into transpond_time from dual;
select message_has_image into message_has_image from Message where message_id=message_transpond_message_id;

insert into Message(message_content, message_create_time, message_agree_num, message_transponded_num,
					message_comment_num, message_view_num, message_has_image, message_sender_user_id, message_heat)
values(message_content, transpond_time, 0, 0, 0, 0, message_has_image, message_sender_user_id, 0);

select message_id into out_id from Message where message_create_time=transpond_time;
message_id:=out_id;

if message_has_image !=0 then
select message_image_count into message_image_count from Message_Image where message_id=message_transpond_message_id;
insert into Message_Image(message_id, message_image_count)values (out_id, message_image_count);
end if;

update Message set message_transponded_num = message_transponded_num + 1, message_heat = message_heat + 1 where message_id = message_transpond_message_id;

if message_source_is_transpond =1 then
select transponded_message_id into transpond_used_id from Transpond where message_id = message_transpond_message_id;
else
transpond_used_id:=message_transpond_message_id;
end if;
insert into Transpond(message_id, transponded_message_id) values (out_id, transpond_used_id);

commit;
return state;
end;
/

-------------------FUNC_DELETE_MESSAGE------------------------
-----------------------??ID????-------------------------------
create or replace 
function 
FUNC_DELETE_MESSAGE(message_id_input in INTEGER, message_has_image_iftrue out INTEGER)
return INTEGER
is 
PRAGMA AUTONOMOUS_TRANSACTION;
state integer:=0;
temp_id integer:=message_id_input;

begin
select count(*) into state 
from MESSAGE 
where message_id = temp_id;

if state != 0 then 
state:=1;
else
select message_has_image into message_has_image_iftrue
from  message
where message_id = temp_id;
delete from MESSAGE 
where message_id = temp_id;
delete from COMMENT_ON_MESSAGE 
where  COMMENT_MESSAGE_ID=temp_id ;
end if;
end;
/

------------FUNC_QUERY_MESSAGE_BY_TOPIC---------------
----------------根据id查找message----------------------
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
    open search_result for 
    SELECT* FROM 
         (SELECT LIKES_MESSAGE_ID
          FROM LIKES
          WHERE LIKES.LIKES_USER_ID=user_id
         ORDER BY LIKES.LIKES_TIME DESC)
    WHERE ROWNUM<=startFrom+limitation
    MINUS
    SELECT* 
    FROM 
         (SELECT LIKES_MESSAGE_ID
          FROM LIKES
          WHERE LIKES.LIKES_USER_ID=user_id
         ORDER BY LIKES.LIKES_TIME DESC)
    WHERE ROWNUM<=startFrom-1;

    state:=1;
  END IF;
	RETURN state;

END;

/

----------------FUNC_SEARCH_MESSAGE-------------------
--------通过搜索键，在Message相关表中搜索相关的推�?------
----返回的属性依次为message_id, message_content, message_create_time, message_agree_num, ---
----message_transponded_num, message_comment_num, message_view_num, message_has_image�?-----
----message_sender_user_id, message_heat,message_image_count,transponded_message_id---------
CREATE OR REPLACE 
FUNCTION FUNC_SEARCH_MESSAGE
(searchKey IN VARCHAR2, startFrom IN INTEGER, limitation IN INTEGER, search_result OUT Sys_refcursor)
RETURN INTEGER
AS
state INTEGER:=1;

BEGIN

  SELECT count(*) into state 
  FROM MESSAGE
  WHERE MESSAGE_CONTENT like'%'||searchKey||'%';

  IF state=0
  THEN
    return state;
  ELSE
    open search_result for 
    SELECT * FROM
         (SELECT *
          FROM (MESSAGE NATURAL left outer join MESSAGE_IMAGE) NATURAL left outer join TRANSPOND
          WHERE MESSAGE_CONTENT like'%'||searchKey||'%'
          ORDER BY MESSAGE_CREATE_TIME DESC)
    WHERE ROWNUM<=startFrom+limitation
    MINUS
    SELECT* FROM
         (SELECT *
          FROM (MESSAGE NATURAL join MESSAGE_IMAGE) NATURAL join TRANSPOND
          WHERE MESSAGE_CONTENT like '%'||searchKey||'%'
          ORDER BY MESSAGE_CREATE_TIME DESC)
    WHERE ROWNUM<=startFrom-1;
    state:=1;
  END IF;

	RETURN state;
END;
/