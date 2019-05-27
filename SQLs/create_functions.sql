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

------------------FUNC_CHECK_USER_EMAIL_EXIST(email in VARCHAR)----------------
-----------------------检查Email是否存在---------------------------
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


------------------FUNC_USER_SIGN_UP----------------
-----------------------注册------------------------
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
(USER_ID, USER_NICKNAME, USER_REGISTER_TIME, USER_SELF_INTRODUCTION, USER_FOLLOWERS_NUM, USER_FOLLOWS_NUM)
values(SEQ_USER_PUBLIC_INFO.nextval, nickname, register_time, set_introduction, '0', '0');

select USER_ID into temp_user_id from USER_PUBLIC_INFO
where USER_NICKNAME = nickname;

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


---------------------FUNC_SEND_MESSAGE-------------------------------
-------------------发布新的推特（添加信息至Message）--------------------
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


------FUNC_QUERY_MESSAGE_IDS_CONTAINS_CERTAIN_TOPIC_ID--------
