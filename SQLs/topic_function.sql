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
own_exist INTEGER:=0;

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

select count(*) into own_exist from Message_Owns_Topic 
where Message_Owns_Topic.message_id = message_id and Message_Owns_Topic.topic_id = temp_topic_id;

if own_exist=0 then
insert into Message_Owns_Topic(message_id, topic_id)values(message_id, temp_topic_id); 
end if;

commit;
return state;
end;
/

--------------------------------------------------
--------------FUNC_QUERY_TOPIC_IDS_BY_HEAT--------------------------------//
create or replace 
FUNCTION FUNC_QUERY_TOPICS_BY_HEAT
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
    open search_result for 
    SELECT* FROM 
         (SELECT *
          FROM TOPIC
         ORDER BY TOPIC.TOPIC_HEAT DESC)
    WHERE ROWNUM<=startFrom+limitation
    MINUS
    SELECT* FROM 
         (SELECT *
          FROM TOPIC
         ORDER BY TOPIC.TOPIC_HEAT DESC)
    WHERE ROWNUM<=startFrom-1;

    state:=1;
  END IF;
	RETURN state;
END;
/


------------------FUNC_SREACH_TOPICS--------------------------------
create or replace 
function func_search_topics
(Searchkey In Varchar2, Startfrom in Integer, Limitation In Integer, Search_Result Out Sys_Refcursor)
return INTEGER
is
state integer:=0;

begin
	select count(*) into state 
  from topic
  where topic_content like '%'||Searchkey||'%';
  
if state!=0 then 
state:=1;
  open search_result for
    select * from(
     (select * from
       (select topic_id
       from topic
       where topic_content like'%'||searchkey||'%'
       order by topic_heat desc)
      where rownum < startfrom+limitation)
     minus
     (select * from
       (select topic_id
       from topic
       where topic_content like'%'||searchkey||'%'
       order by topic_heat desc)
      where rownum < startfrom)
    )
    where rownum >= startfrom and rownum <= limitation;   
end if;

return state;
end;
/

-------------------FUNC_GET_TOPIC_ID_BY_NAME----------------------------
-------------------通过话题内容来获得ID---------------------
create or replace function func_get_topic_id_by_name
(Searchkey In Varchar2, Search_Result Out Integer)
return INTEGER
is
state integer:=0;

begin
	select count(*) into state 
  from topic
  where topic_content = Searchkey;
  
if state!=0 then 
state:=1;
  select topic_id into Search_Result
  from TOPIC
  where TOPIC_CONTENT = Searchkey;
end if;

return state;
end;
/


-----FUNC_QUERY_MESSAGE_BY_TOPIC
create or replace 
function
FUNC_QUERY_MESSAGE_BY_TOPIC(topic_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
RETURN INTEGER
is
state INTEGER:=1;
m_topic_id INTEGER:=topic_id;
BEGIN

	SELECT count(*) into state 
  from MESSAGE_OWNS_TOPIC
  WHERE MESSAGE_OWNS_TOPIC.TOPIC_ID=topic_id;


    open search_result for
    select * from
    ( 
      select Message_Owns_Topic.message_id from Message_owns_topic
      where Message_Owns_Topic.topic_id = m_topic_id)
    where rownum <= limitation+startFrom
      minus
    select * from
    (
      select Message_Owns_Topic.message_id from Message_owns_topic
      where Message_Owns_Topic.topic_id = m_topic_id)
    where rownum <= startFrom - 1;
    state:=1;

	RETURN state;

END;
/