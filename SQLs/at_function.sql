---------------FUNC_QUERY_MESSAGE_AT_USER----------------------
------------------------查询@我的信息----------------------------------
create or replace 
function 
FUNC_QUERY_MESSAGE_AT_USER(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
state INTEGER:=0;

begin
select count(*) into state
from AT_USER
where AT_USER_ID = user_id;

if state=0 then 
return state;
else
state:=1;
open search_result for
(
select * from(
(select MESSAGE_ID
from AT_USER
where AT_USER_ID= user_id
order by AT_TIME desc)
where ROWNUM <startFrom+limitation)
minus
(select * from(
select MESSAGE_ID
from AT_USER
where AT_USER_ID= user_id
order by AT_TIME desc)
where ROWNUM < startFrom)
)


end if;
return state;
end;
/

-------------FUNC_ADD_AT_USER-----------------------------------
-------------添加@某人的记录------------------------------------
create or replace
function
FUNC_ADD_AT_USER(at_nickname in VARCHAR2, message_id in INTEGER, source_user_id in INTEGER)
return INTEGER
is
state INTEGER:=0;
count_nickname INTEGER;
temp_time VARCHAR2(30);

begin 

select to_char(sysdate, 'yyyy-mm-dd HH24:MI:SS')into temp_time from dual;

select count(*) into count_nickname
from USER_PUBLIC_INFO
where user_nickname = at_nickname;

if count_nickname = 0
then state:=1;
return state;
else 
insert into AT_USER(at_user_id, message_id, user_id, at_time, at_is_read)
values ((select user_id from USER_PUBLIC_INFO where user_nickname=at_nickname), message_id, source_user_id, temp_time, 0);
state:=1;
end if;

return state;
end;
/









