------------------FUNC_ADD_RELATION----------------------
------------------添加用户关系---------------------------
create or replace function
FUNC_ADD_RELATION(follower_id in INTEGER, be_followed_id in INTEGER)
return INTEGER
is 
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
create_time VARCHAR2(30);

begin 
select to_char(sysdate,'yyyy-mm-dd HH24:MI:SS')into create_time from dual;

insert into Relation(relation_create_time, relation_user_follower_id, relation_user_be_followed_id)
values(create_time, follower_id, be_followed_id);

update User_Public_Info set user_follows_num = user_follows_num + 1 where user_id = follower_id;
update User_Public_Info set user_followers_num = user_followers_num + 1 where user_id = be_followed_id;

commit;
return state;
end;
/


---------------FUNC_REMOVE_RELATION----------------------
---------------删除关注关系------------------------------
create or replace function
FUNC_REMOVE_RELATION(follower_id in INTEGER, be_followed_id in INTEGER)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
judage_exist INTEGER:=0;

begin
select count(*) into judage_exist from Relation 
where relation_user_follower_id = follower_id and relation_user_be_followed_id = be_followed_id;
if judage_exist = 0 then 
state:=0;
else
delete from Relation where relation_user_follower_id = follower_id and relation_user_be_followed_id = be_followed_id;
update User_Public_Info set user_follows_num = user_follows_num - 1 where user_id = follower_id;
update User_Public_Info set user_followers_num = user_followers_num - 1 where user_id = be_followed_id;
end if;
commit;
return state;
end;
/

---------------FUNC_QUERY_FOLLOWING_LIST----------------------
-------------------------查找关注列表---------------------------------
create or replace 
function 
FUNC_QUERY_FOLLOWING_LIST(my_user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
id_temp NUMBER(38,0);
state INTEGER:=0;
create_time VARCHAR2(30);
begin



select count(*) into state
from RELATION 
where RELATION_USER_FOLLOWER_ID = my_user_id;

if state!=0 then 
state:=1;
end if;

open search_result for
select* from(
select user_id, user_nickname
from USER_PUBLIC_INFO,RELATION 
where user_id=RELATION.RELATION_USER_BE_FOLLOWED_ID and
my_user_id=RELATION.RELATION_USER_FOLLOWER_ID
order by RELATION_CREATE_TIME desc)
where ROWNUM <startFrom+limitation
minus
select* from(
select user_id, user_nickname
from USER_PUBLIC_INFO,RELATION 
where user_id=RELATION.RELATION_USER_BE_FOLLOWED_ID and
my_user_id=RELATION.RELATION_USER_FOLLOWER_ID
order by RELATION_CREATE_TIME desc)
where ROWNUM <startFrom;


return state;
end;
/

---------------FUNC_QUERY_FOLLOWERS_LIST----------------------
-------------------------查找粉丝列表---------------------------------
create or replace 
function 
FUNC_QUERY_FOLLOWERS_LIST(my_user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
id_temp NUMBER(38,0);
state INTEGER:=0;
create_time VARCHAR2(30);
begin


select count(*) into state
from RELATION 
where relation_user_be_followed_id = my_user_id;

if state!=0 then 
state:=1;
end if;
open search_result for
select* from(
select user_id, user_nickname
from USER_PUBLIC_INFO,RELATION 
where my_user_id=RELATION.RELATION_USER_BE_FOLLOWED_ID and
user_id=RELATION.RELATION_USER_FOLLOWER_ID
order by RELATION_CREATE_TIME desc)
where ROWNUM <startFrom+limitation
minus
select* from(
select user_id, user_nickname
from USER_PUBLIC_INFO,RELATION 
where my_user_id=RELATION.RELATION_USER_BE_FOLLOWED_ID and
user_id=RELATION.RELATION_USER_FOLLOWER_ID
order by RELATION_CREATE_TIME desc)
where ROWNUM <startFrom;


return state;
end;
/