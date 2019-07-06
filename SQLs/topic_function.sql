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
insert into Topic(topic_heat, topic_content) 
values ( 1, topic_name);
select topic_id into temp_topic_id from Topic where topic_content=topic_name;
commit;
end if;

insert into Message_Owns_Topic(message_id, topic_id)values(message_id, temp_topic_id); 

commit;
return state;
end;
/

--------------------------------------------------
--------------FUNC_QUERY_TOPIC_IDS_BY_HEAT--------------------------------//

CREATE OR REPLACE 
FUNCTION FUNC_QUERY_TOPIC_IDS_BY_HEAT
(startFrom IN INTEGER, limitation IN INTEGER, search_result OUT sys_refcursor)
RETURN INTEGER
AS
state INTEGER:=1;

BEGIN

	SELECT count(*) into state 
  from TOPIC;

  IF state=0
  THEN 
    return state;
  ELSE  
    open search_result for SELECT* FROM 
         (SELECT TOPIC_ID
          FROM TOPIC
          WHERE TOPIC.TOPIC_ID>=startFrom
         ORDER BY TOPIC.TOPIC_HEAT DESC)
    WHERE ROWNUM<=limitation;

    state:=1;
  END IF;
	RETURN state;
END;
/