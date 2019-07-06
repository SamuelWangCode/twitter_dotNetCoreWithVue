------------------FUNC_CHECK_USER_EMAIL_EXIST(email in VARCHAR)----------------
-----------------------检查用户Email是否存在于数据库中---------------------------
create or replace function 
FUNC_CHECK_USER_EMAIL_EXIST(email in VARCHAR)
return INTEGER
is 
state INTEGER;
begin 
select count(*)
into state
from USER_PRIVATE_INFO
where USER_EMAIL=email;
return state;
end;
/

------------------FUNC_CHECK_USER_ID_EXIST(email in VARCHAR)----------------
-----------------------检查用户id是否存在于数据库---------------------------
create or replace function 
FUNC_CHECK_USER_ID_EXIST(userid in VARCHAR)
return INTEGER
is 
state INTEGER;
begin 
select count(*)
into state
from USER_PUBLIC_INFO
where userid=USER_ID;
return state;
end;
/

------------------FUNC_USER_SIGN_UP----------------
-----------通过给定的用户信息向数据库添加新用户-------
create or replace function 
FUNC_USER_SIGN_UP(email in VARCHAR, nickname in VARCHAR, password in VARCHAR)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;

state INTEGER :=1;
temp_user_id INTEGER :=0;
register_time VARCHAR2(30) ;
set_introduction VARCHAR2(255) :='The man is lazy,leaving nothing.';
begin
select to_char(sysdate, 'yyyy-mm-dd HH24:MI:SS') into register_time from dual;

insert into USER_PUBLIC_INFO
(USER_NICKNAME, USER_REGISTER_TIME, USER_SELF_INTRODUCTION, USER_FOLLOWERS_NUM, USER_FOLLOWS_NUM)
values(nickname, register_time, set_introduction, '0', '0');

select USER_ID into temp_user_id from USER_PUBLIC_INFO
where USER_REGISTER_TIME = register_time;

insert into USER_PRIVATE_INFO(USER_ID, USER_EMAIL, USER_PASSWORD)
values(temp_user_id, email, password);

commit;
return state;

end;
/

------------------FUNC_USER_SIGN_IN----------------
----------------------验证登录----------------------
create or replace function FUNC_USER_SIGN_IN_BY_EMAIL(email in VARCHAR, password in VARCHAR, re_user_id out INTEGER)
return INTEGER
is 
state INTEGER;
begin

select count(*)
into state
from USER_PRIVATE_INFO
where email=USER_EMAIL AND password=USER_PASSWORD;

if state=1 then
select USER_ID
into re_user_id
from USER_PRIVATE_INFO
where email=USER_EMAIL AND password=USER_PASSWORD;
end if;
return state;
end;
/


---------------FUNC_SET_USER_INFO----------------
------------------设置个人信息--------------------
create or replace function 
FUNC_SET_USER_INFO
(nickname in VARCHAR, self_introduction in VARCHAR, password in VARCHAR, realname in VARCHAR, gender in VARCHAR,id in INTEGER, set_mode in INTEGER)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
t_nickname VARCHAR(20);
t_password VARCHAR(20); 
t_realname VARCHAR(20);
t_gender VARCHAR(4);
t_self_introduction VARCHAR(255);
state INTEGER;
begin
state:=FUNC_CHECK_USER_ID_EXIST(id);
if state=0 then 
return state;
end if;

select USER_NICKNAME,USER_SELF_INTRODUCTION
into t_nickname,t_self_introduction
from USER_PUBLIC_INFO
where USER_ID=id;

select USER_PASSWORD,USER_REAL_NAME,USER_GENDER
into t_password,t_realname,t_gender
from USER_PRIVATE_INFO
where USER_ID=id;

if bitand(set_mode,1)=1 then
t_nickname:=nickname;
end if;
if bitand(set_mode,2)=2 then
t_self_introduction:=self_introduction;
end if;
if bitand(set_mode,4)=4 then
t_password:=password;
end if;
if bitand(set_mode,8)=8 then
t_realname:=realname;
end if;
if bitand(set_mode,16)=16 then
t_gender:=gender;
end if;

update USER_PUBLIC_INFO
set USER_NICKNAME=t_nickname,USER_SELF_INTRODUCTION=t_self_introduction
where USER_ID=id;

update USER_PRIVATE_INFO
set USER_PASSWORD=t_password,USER_REAL_NAME=t_realname,USER_GENDER=t_gender
where USER_ID=id;
commit;
return state;
end;
/


---------------------FUNC_SET_MAIN_AVATAR-------------------------
---------------------------设置头像--------------------------------
create or replace function FUNC_SET_MAIN_AVATAR(userid in INTEGER, avatarid in INTEGER)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
temp INTEGER;
begin

state:=FUNC_CHECK_USER_ID_EXIST(userid);

if state=0 then
return state;
end if;

update Avatar_Image
set avatar_image_in_use=0
where userid=USER_ID;
select count(*) into temp from Avatar_Image where userid=USER_ID AND avatarid=avatar_image_id;
if temp!=0 then
update Avatar_Image
set avatar_image_in_use=1
where userid=USER_ID AND avatarid=avatar_image_id;
else 
insert into Avatar_Image (USER_ID,avatar_image_id,avatar_image_in_use)
values (userid,avatarid,1);
end if;
commit;
return state;
end;
/

---------------------FUNC_GET_USER_PUBLIC_INFO-------------------------
--------------------获取个人公开信息--------------------------------
create or replace function FUNC_GET_USER_PUBLIC_INFO(userid in INTEGER, info out sys_refcursor)
return INTEGER
is
state INTEGER:=1;
begin
state:=FUNC_CHECK_USER_ID_EXIST(userid);
if state=0 then
return state;
end if;

open info for 
select user_id,user_nickname,user_register_time,user_self_introduction,user_followers_num,user_follows_num
from USER_PUBLIC_INFO where user_id=userid;

return state;
end;
/

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
/

---------------FUNC_SEARCH_USER----------------------------
---------------搜索用户信息--------------------------------
CREATE OR REPLACE
function FUNC_SEARCH_USER
(searchKey in VARCHAR2, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
state INTEGER:=1;

begin

select count(*) into state 
from USER_PUBLIC_INFO;

if state=0
then return state;
else 
state:=1;

open search_result for 
select * from (
(select user_id, user_nickname
from (select user_id, user_nickname
	 from USER_PUBLIC_INFO 
	 where user_nickname like searchKey 
	 order by user_followers_num desc)
where ROWNUM<(startFrom+limitation)) 
minus 
(select user_id, user_nickname
from (select user_id, user_nickname
	 from USER_PUBLIC_INFO 
	 where user_nickname like searchKey 
	 order by user_followers_num desc)
where ROWNUM<startFrom)
);

end if;
return state;
end;
/




