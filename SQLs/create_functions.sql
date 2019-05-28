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


---------------FUNC_SET_USER_INFO----------------
------------------修改用户信息--------------------
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
select count(*) into state from USER_PRIVATE_INFO where USER_ID=id;
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
