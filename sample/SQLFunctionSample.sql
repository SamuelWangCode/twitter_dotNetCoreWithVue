create or replace 
function FUNC_SEARCH_TOPIC_BY_HEAT(heat in INTEGER, search_result out sys_refcursor)
return INTEGER
is
result_size INTEGER :=0;
state INTEGER :=1;
begin 
  select count(*) into result_size from TOPIC where TOPIC_HEAT=heat;
  open search_result for select * from TOPIC where TOPIC_HEAT=heat;
  if result_size=0 then
    state :=0;
  else 
    state :=1;
  end if;
  return state;
end;