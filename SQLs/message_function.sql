---------------FUNC_SHOW_MESSAGE_BY_ID----------------------
------------------根据ID查询推特信息-------------------------------
create or replace 
function 
FUNC_SHOW_MESSAGE_BY_ID(message_id in INTEGER, result out sys_refcursor)
return INTEGER
IS
state INTEGER:=0;
c1 SYS_REFCURSOR;

begin
open c1 for
select *
from MESSAGE
where message_id=MESSAGE.message_id;

select count(*) into state 
from MESSAGE
where message_id=MESSAGE.message_id;
if state!=0 then
state:=1;
end if;

return state;
end;
/


---------------FUNC_SHOW_ MESSAGE_BY_RANGE----------------------
-----------------------根据索引查询推特信息-------------------------------
create or replace 
function 
FUNC_SHOW_MESSAGE_BY_RANGE(user_id in INTEGER, rangeStart in INTEGER, rangeLimitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
state INTEGER:=0;

begin
select count(*) into state
from MESSAGE 
where MESSAGE_SENDER_USER_ID = user_id;

if state=0 then 
return state;
else
state:=1;
open search_result for 
select * from(
select *
from MESSAGE
where MESSAGE_SENDER_USER_ID= user_id)
where ROWNUM >= rangeStart and ROWNUM <= rangeLimitation;

end if;
return state;
end;
/





---------------------FUNC_SEND_MESSAGE-------------------------------
---------------------发布新的推特（添加信息至Message）---------------
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
insert into Message(message_id, message_content, message_create_time, message_agree_num, message_transponded_num,
					message_comment_num, message_view_num, message_has_image, message_sender_user_id, message_heat)
values(seq_message.nextval, message_content, create_time, '0', '0', '0', '0', message_has_image, user_id, '0');

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
-------------------转发1条推特（Message和Transpond添加）
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

insert into Message(message_id, message_content, message_create_time, message_agree_num, message_transponded_num,
					message_comment_num, message_view_num, message_has_image, message_sender_user_id, message_heat)
values(seq_message.nextval, message_content, transpond_time, 0, 0, 0, 0, message_has_image, message_sender_user_id, 0);

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
-----------------------根据ID删除推特-------------------------------
create or replace 
function 
FUNC_DELETE_MESSAGE(message_id in INTEGER, message_has_image out INTEGER)
return INTEGER
is 
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=0;

begin
select count(*) into state 
from MESSAGE 
where message_id = MESSAGE.message_id;

if state != 0 then 
state:=1;
else
delete from MESSAGE 
where message_id = MESSAGE.message_id;
delete from COMMENT_ON_MESSAGE 
where message_id = COMMENT_MESSAGE_ID;
end if;
end;
/

------------FUNC_QUERY_MESSAGE_BY_TOPIC---------------
----------------根据id查找message----------------------
create or replace 
function
FUNC_QUERY_MESSAGE_BY_TOPIC(topic_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
RETURN INTEGER
is
state INTEGER:=1;

BEGIN

	SELECT count(*) into state 
  from MESSAGE_OWNS_TOPIC
  WHERE MESSAGE_OWNS_TOPIC.TOPIC_ID=topic_id;

  IF state=0
  THEN 
    return state;
  ELSE  
    open search_result for SELECT message_id FROM 
         (SELECT MESSAGE_OWNS_TOPIC.message_id
          FROM MESSAGE, MESSAGE_OWNS_TOPIC
          WHERE MESSAGE_OWNS_TOPIC.message_id>=startFrom
              AND MESSAGE_OWNS_TOPIC.TOPIC_ID=topic_id
			  AND MESSAGE.message_id=MESSAGE_OWNS_TOPIC.message_id
         ORDER BY MESSAGE.MESSAGE_CREATE_TIME DESC)
    WHERE ROWNUM<=limitation;

    state:=1;
  END IF;
	RETURN state;

END;
/