---------------FUNC_QUERY_MESSAGE_AT_USER----------------------
------------------------查询@我的信息----------------------------------
create or replace 
function 
FUNC_QUERY_MESSAGE_AT_USER(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
return INTEGER
is 
state INTEGER:=0;

begin
select count(*) into state
from AT_USER
where AT_USER_ID = user_id;

if state=0 then 
return state;
else
state:=1;
open search_result for 
select * from(
select MESSAGE_ID
from AT_USER
where AT_USER_ID= user_id
order by AT_TIME desc)
where ROWNUM >= startFrom and ROWNUM <= limitation;

end if;
return state;
end;
/