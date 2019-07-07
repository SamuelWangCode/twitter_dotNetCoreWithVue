--------------------------------------------------
--------------FUNC_GET_USER_AVATAR--------------------------------//
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

if state>0
  then 
  state:=1;
  select 
  avatar_image_id into avatar_id 
  from Avatar_Image
  where user_id=Avatar_Image.user_id and avatar_image_in_use=1;
  return state;
end if;
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
---------------ADD_AVATAR
create or replace function ADD_AVATAR(user_id in INTEGER, avatar_id out INTEGER)
return INTEGER
is
PRAGMA AUTONOMOUS_TRANSACTION;
state INTEGER:=1;
m_user_id INTEGER:=user_id;
begin

insert into AVATAR_IMAGE(USER_ID, AVATAR_IMAGE_IN_USE) VALUES (m_user_id, 0) returning AVATAR_IMAGE_ID into avatar_id;
commit;
return state;
end;
/
