CREATE OR REPLACE TRIGGER trigger_check_transpond_message_id
  BEFORE INSERT ON Message
  FOR EACH ROW
  DECLARE 
    temp INTEGER;
  BEGIN
    IF 
      :NEW.message_is_transpond = 1
    THEN
      (SELECT COUNT(*) INTO temp
        FROM Message Msg
        WHERE :NEW.message_transpond_message_id = Msg.message_id);
      IF temp = 0 THEN
        RAISE_APPLICATION_ERROR(-20000, '转发的消息不存在');
      END IF;
    END IF;
  END;