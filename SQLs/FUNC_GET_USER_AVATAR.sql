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
return state;

if state>0
  then 
  state:=1;
  select 
  avatar_image_id into avatar_id 
  from Avatar_Image
  where user_id=Avatar_Image.user_id and avatar_image_in_use=1;
  return state;
  else 
  state:=0;
end if;

end;
