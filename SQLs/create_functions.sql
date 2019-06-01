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
\

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
\

-------------------FUNC_ADD_TOPIC----------------------------
-------------------添加话题/增加话题热度---------------------
create or replace function
FUNC_ADD_TOPIC(topic_name in VARCHAR2, message_id in INTEGER)
return INTEGER
is 
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
temp_topic_id INTEGER:=-1;
topic_exist INTEGER:=0;

begin
select count(*) into topic_exist from Topic where topic_content = topic_name;
if topic_exist !=0 then
select topic_id into temp_topic_id from Topic where topic_content=topic_name;
update Topic set topic_heat = topic_heat + 1 where topic_id=temp_topic_id;
commit;
end if;

if topic_exist=0 then
insert into Topic(topic_id, topic_heat, topic_content) 
values (seq_topic.nextval, 1, topic_name);
select topic_id into temp_topic_id from Topic where topic_content=topic_name;
commit;
end if;

insert into Message_Owns_Topic(message_id, topic_id)values(message_id, temp_topic_id); 

commit;
return state;
end;
\

-------------------FUNC_TRANSPOND_MESSAGE--------------------
-------------------转发一条推特（Message和Transpond添加）
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
\

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
\

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
\


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
\

--------------FUNC_ADD_COMMENT---------------------------
--------------添加评论-----------------------------------
create or replace function
FUNC_ADD_COMMENT(user_id in INTEGER, be_commented_id in INTEGER, content in VARCHAR2)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGEr:=1;
create_time VARCHAR2(30);
own_topic_num INTEGER:=0;

begin
select to_char(sysdate,'yyyy-mm-dd HH24:MI:SS')into create_time from dual;

insert into 
Comment_On_Message(comment_id, comment_content, comment_is_read, comment_sender_id, comment_message_id, comment_create_time)
values (seq_comment_on_Message.nextval, content, 0, user_id, be_commented_id, create_time);

update Message set message_comment_num = message_comment_num + 1, message_heat = message_heat + 1 
where message_id = be_commented_id;

select count(*) into own_topic_num from Message_Owns_Topic where message_id = be_commented_id;

if own_topic_num >0 then 
update Topic set topic_heat = topic_heat + 1
where topic_id in (select topic_id from Message_Owns_Topic
					where message_id = be_commented_id);
end if;
commit;
return state;
end;
\

---------------FUNC_ADD_COLLECTION----------------------
---------------添加收藏---------------------------------
create or replace function
FUNC_ADD_COLLECTION(user_id in INTEGER, be_collected_id in INTEGER)
return INTEGER
is 
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
own_topic_num INTEGER:=0;

begin
insert into Message_Collection(user_id, message_id)
values(user_id, be_collected_id);

update Message set message_heat = message_heat + 1 where message_id = be_collected_id; 

select count(*) into own_topic_num from Message_Owns_Topic where message_id = be_collected_id;

if own_topic_num >0 then 
update Topic set topic_heat = topic_heat + 1
where topic_id in (select topic_id from Message_Owns_Topic
					where message_id = be_collected_id);
end if;
commit;
return state;
end;
\

---------------FUNC_DELETE_COLLECTION----------------------
-----------------------删除收藏----------------------------------
create or replace 
function 
FUNC_DELETE_COLLECTION(user_id in INTEGER, message_id in INTEGER)
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
delete from MESSAGE_COLLECTION
where message_id = MESSAGE_COLLECTION.message_id and user_id=MESSAGE_COLLECTION.user_id;
update MESSAGE 
set MESSAGE_HEAT=MESSAGE_HEAT-1
where message_id = MESSAGE.message_id;
update TOPIC 
set TOPIC_HEAT=TOPIC_HEAT-1
where topic_id=( select TOPIC_ID 
from MESSAGE_OWNS_TOPIC
where message_id = MESSAGE_OWNS_TOPIC.message_id);
end if;

commit;
return state;
end;
\