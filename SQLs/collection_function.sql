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
/

---------------FUNC_DELETE_COLLECTION----------------------
-----------------------删除收藏----------------------------------
create or replace 
function 
FUNC_DELETE_COLLECTION(user_id_input in INTEGER, message_id_input in INTEGER)
return INTEGER
is 
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=0;

begin
select count(*) into state 
from message 
where message_id = message_id_input;

if state != 0 then 
state:=1;
else
delete from message_collection
where message_id = message_id_input and user_id=user_id_input;
update MESSAGE 
set message_heat=message_heat-1
where message_id =message_id_input;
update TOPIC 
set TOPIC_HEAT=TOPIC_HEAT-1
where topic_id=( select TOPIC_ID 
from message_owns_topic
where message_id = message_id_input);
end if;

commit;
return state;
end;
/



---------------FUNC_QUERY_COLLECTIONS_OF_MINE--------------------
-----------------------------查询收藏信息----------------------------------
create or replace 
function 
FUNC_QUERY_COLLECTIONS_OF_MINE(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
state INTEGER:=0;

begin
select count(*) into state
from MESSAGE_COLLECTION
where MESSAGE_COLLECTION.USER_ID = user_id;

if state=0 then 
return state;
else
state:=1;
open search_result for
(
select * from(
(select MESSAGE_COLLECTION.MESSAGE_ID
from MESSAGE_COLLECTION
where MESSAGE_COLLECTION.USER_ID= user_id)
where ROWNUM <startFrom+limitation)
minus
select * from(
(select MESSAGE_COLLECTION.MESSAGE_ID
from MESSAGE_COLLECTION
where MESSAGE_COLLECTION.USER_ID= user_id)
where ROWNUM <startFrom)
)

end if;
return state;
end;
/