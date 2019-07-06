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
FUNC_QUERY_FOLLOWING_LIST(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
id_temp NUMBER(38,0);
state INTEGER:=0;
create_time VARCHAR2(30);
begin

select RELATION_CREATE_TIME 
into create_time
from RELATION;

select count(*) into state
from RELATION 
where RELATION_USER_FOLLOWER_ID = user_id;

if state=0 then 
return state;
else
state:=1;

open search_result for
select* from(
select USER_PUBLIC_INFO.user_id, USER_PUBLIC_INFO.user_nickname,AVATAR_IMAGE.AVATAR_IMAGE_ID
from USER_PUBLIC_INFO,AVATAR_IMAGE
where AVATAR_IMAGE.USER_ID=USER_PUBLIC_INFO.USER_ID and AVATAR_IMAGE.AVATAR_IMAGE_IN_USE=1
and id_temp in(
select RELATION.RELATION_USER_BE_FOLLOWED_ID 
from RELATION
where id_temp=RELATION.RELATION_USER_FOLLOWER_ID)
order by create_time desc)
 
where ROWNUM >= startFrom and ROWNUM <= limitation;

end if;
return state;
end;
/

---------------FUNC_QUERY_FOLLOWING_LIST----------------------
-------------------------查找粉丝列表---------------------------------
create or replace 
function 
FUNC_QUERY_FOLLOWERS_LIST(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
id_temp NUMBER(38,0);
state INTEGER:=0;
create_time VARCHAR2(30);
begin

select RELATION_CREATE_TIME 
into create_time
from RELATION;

select count(*) into state
from RELATION 
where RELATION_USER_FOLLOWER_ID = user_id;

if state=0 then 
return state;
else
state:=1;

open search_result for
select* from(
select USER_PUBLIC_INFO.user_id, USER_PUBLIC_INFO.user_nickname,AVATAR_IMAGE.AVATAR_IMAGE_ID
from USER_PUBLIC_INFO,AVATAR_IMAGE
where AVATAR_IMAGE.USER_ID=USER_PUBLIC_INFO.USER_ID and AVATAR_IMAGE.AVATAR_IMAGE_IN_USE=1
and id_temp in(
select RELATION.RELATION_USER_FOLLOWER_ID
from RELATION
where id_temp=RELATION.RELATION_USER_BE_FOLLOWED_ID)
order by create_time desc)
 
where ROWNUM >= startFrom and ROWNUM <= limitation;

end if;
return state;
end;
/