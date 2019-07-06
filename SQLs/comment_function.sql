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
/