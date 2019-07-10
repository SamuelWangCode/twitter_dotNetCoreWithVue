DROP TABLE "AVATAR_IMAGE" CASCADE CONSTRAINTS;
DROP TABLE "AT_USER" CASCADE CONSTRAINTS;
DROP TABLE "COMMENT_ON_MESSAGE" CASCADE CONSTRAINTS;
DROP TABLE "LIKES" CASCADE CONSTRAINTS;
DROP TABLE "MESSAGE" CASCADE CONSTRAINTS;
DROP TABLE "MESSAGE_COLLECTION" CASCADE CONSTRAINTS;
DROP TABLE "MESSAGE_IMAGE" CASCADE CONSTRAINTS;
DROP TABLE "MESSAGE_OWNS_TOPIC" CASCADE CONSTRAINTS;
DROP TABLE "PRIVATE_LETTER" CASCADE CONSTRAINTS;
DROP TABLE "RELATION" CASCADE CONSTRAINTS;
DROP TABLE "TOPIC" CASCADE CONSTRAINTS;
DROP TABLE "USER_PRIVATE_INFO" CASCADE CONSTRAINTS;
DROP TABLE "USER_PUBLIC_INFO" CASCADE CONSTRAINTS;
DROP TABLE "TRANSPOND" CASCADE CONSTRAINTS;

DROP SEQUENCE  SEQ_AT_USER;
DROP SEQUENCE  SEQ_AVATAR_IMAGE;
DROP SEQUENCE  SEQ_COMMENT_ON_MESSAGE;
DROP SEQUENCE  SEQ_MESSAGE;
DROP SEQUENCE  SEQ_MESSAGE_IMAGE;
DROP SEQUENCE  SEQ_PRIVATE_LETTER;
DROP SEQUENCE  SEQ_TOPIC;
DROP SEQUENCE  SEQ_LIKES;
DROP SEQUENCE  SEQ_USER_PUBLIC_INFO;


----------User_Public_Info----------------


CREATE TABLE User_Public_Info (
  user_id INTEGER GENERATED ALWAYS AS IDENTITY,
  user_nickname VARCHAR2(20) NOT NULL UNIQUE,
  user_register_time VARCHAR2(30) NOT NULL,
  user_self_introduction VARCHAR2(255) NOT NULL,
  user_followers_num INTEGER NOT NULL,
  user_follows_num INTEGER NOT NULL,
  CONSTRAINT pk_user_public_info 
	PRIMARY KEY(user_id)
);




-------------------------------------------
--------------User_Private_Info------------

--!!!!重要 User_Private_Info不需要用Sequence来表示主键，插入新数据时，使用对应的User_Public_Info中的user_id即可
CREATE TABLE User_Private_Info(
  user_id INTEGER PRIMARY KEY,
  user_password VARCHAR2(20) NOT NULL,
  user_gender VARCHAR2(6),
  user_real_name VARCHAR2(20),
  user_email VARCHAR2(50) NOT NULL UNIQUE,
  CONSTRAINT fk_user_private_info FOREIGN KEY (user_id)
      REFERENCES User_Public_Info (user_id)
        ON DELETE CASCADE
);

------------------------------------------
------------Avatar_Image-------------

CREATE TABLE Avatar_Image （
  avatar_image_id INTEGER GENERATED ALWAYS AS IDENTITY,
  user_id INTEGER,
  avatar_image_in_use INTEGER NOT NULL,
  CONSTRAINT pk_avatar_image
	PRIMARY KEY (avatar_image_id，user_id),
  CONSTRAINT fk_avatar_image FOREIGN KEY (user_id)
	REFERENCES USER_Public_INFO (user_id)
		ON DELETE CASCADE
);



------------------------------------------

-----------------Private_Letter-----------

CREATE TABLE Private_Letter(
  private_letter_id INTEGER GENERATED ALWAYS AS IDENTITY,
  private_letter_content VARCHAR2(255) NOT NULL,
  private_letter_is_read INTEGER NOT NULL,
  private_letter_create_time VARCHAR2(30) NOT NULL,
  private_letter_sender_id INTEGER NOT NULL,
  private_letter_receiver_id INTEGER NOT NULL,
  CONSTRAINT pk_private_letter 
	PRIMARY KEY (private_letter_id),
  CONSTRAINT fk_private_letter_1
    FOREIGN KEY (private_letter_sender_id)
      REFERENCES User_Public_Info(user_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_private_letter_2
    FOREIGN KEY (private_letter_receiver_id)
      REFERENCES User_Public_Info(user_id)
        ON DELETE CASCADE
);

CREATE INDEX pl_sender_index ON Private_Letter(private_letter_sender_id);
CREATE INDEX pl_receiver_index ON Private_Letter(private_letter_receiver_id);
CREATE INDEX pl_create_time ON Private_Letter(private_letter_create_time);
---------------------------------------------
----------------Relation---------------------
CREATE TABLE Relation(
  relation_create_time VARCHAR2(30) NOT NULL,
  relation_user_follower_id INTEGER NOT NULL,
  relation_user_be_followed_id INTEGER NOT NULL,
  CONSTRAINT pk_relation
    PRIMARY KEY (relation_user_follower_id, relation_user_be_followed_id),
  CONSTRAINT fk_relation_1
    FOREIGN KEY (relation_user_follower_id)
      REFERENCES User_Public_Info(user_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_relation_2
    FOREIGN KEY (relation_user_be_followed_id)
      REFERENCES User_Public_Info(user_id)
        ON DELETE CASCADE
);
---------------------------------------------
-------------Message-------------------------
CREATE TABLE Message(
  message_id INTEGER GENERATED ALWAYS AS IDENTITY,
  message_content VARCHAR2(560) NOT NULL,
  message_create_time VARCHAR2(30) NOT NULL,
  message_agree_num INTEGER NOT NULL,
  message_transponded_num INTEGER NOT NULL,
  message_comment_num INTEGER NOT NULL,
  message_view_num INTEGER NOT NULL,
  message_has_image INTEGER NOT NULL,
  message_sender_user_id INTEGER NOT NULL,
  message_heat INTEGER NOT NULL,
  CONSTRAINT pk_message 
	PRIMARY KEY (message_id),
  CONSTRAINT fk_message
    FOREIGN KEY (message_sender_user_id)
      REFERENCES User_Public_Info(user_id)
        ON DELETE CASCADE
);


CREATE INDEX m_sender_user_id ON Message(message_sender_user_id, message_create_time);
CREATE INDEX m_create_time ON Message(message_create_time);
CREATE INDEX m_heat_create_time ON Message(message_heat, message_create_time);
----------------------------------------------
-----------------Transpond--------------------
CREATE TABLE Transpond(
	message_id INTEGER,
	transponded_message_id INTEGER,
	CONSTRAINT pk_Transpnd
		PRIMARY KEY (message_id, transponded_message_id),
	CONSTRAINT fk_message_id
		FOREIGN KEY (message_id)
			REFERENCES Message(message_id)
				ON DELETE CASCADE,
	CONSTRAINT fk_transponded_message_id
		FOREIGN KEY (transponded_message_id)
			REFERENCES Message(message_id)
				ON DELETE CASCADE
);
----------------------------------------------
------------------Message_Image---------------

CREATE TABLE Message_Image(
  message_id INTEGER NOT NULL,
  message_image_count INTEGER NOT NULL,
  CONSTRAINT pk_message_image
	PRIMARY KEY (message_id),
  CONSTRAINT fk_message_image FOREIGN KEY
    (message_id) REFERENCES Message(message_id)
      ON DELETE CASCADE
);
-------------------------------------------------
------------------At_User------------------------

CREATE TABLE At_User(
  at_user_id INTEGER,
  message_id INTEGER,
  user_id INTEGER NOT NULL,
  at_time VARCHAR2(30) NOT NULL,
  at_is_read INTEGER NOT NULL,
  PRIMARY KEY(at_user_id, message_id),
  CONSTRAINT fk_at_user_1 FOREIGN KEY
    (message_id) REFERENCES
      Message(message_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_at_user_2 FOREIGN KEY
    (user_id) REFERENCES
      User_Public_Info(user_id)
        ON DELETE CASCADE
);

CREATE INDEX a_u_user_id_index ON At_User(user_id, at_time);
--------------------------------------------------
--------------Like--------------------------------

CREATE TABLE Likes(
  likes_id INTEGER GENERATED ALWAYS AS IDENTITY,
  likes_user_id INTEGER NOT NULL,
  likes_message_id INTEGER NOT NULL,
  likes_time VARCHAR2(30) NOT NULL,
  CONSTRAINT pk_likes
	PRIMARY KEY (likes_id),
  CONSTRAINT fk_likes_1 FOREIGN KEY
    (likes_user_id) REFERENCES
      User_Public_Info(user_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_likes_2 FOREIGN KEY
    (likes_message_id) REFERENCES
      Message(message_id)
        ON DELETE CASCADE
);

CREATE INDEX l_user_id_index ON Likes(likes_user_id, likes_time);
CREATE INDEX l_message_id_index ON Likes(likes_message_id, likes_time);

-----------------Comment--------------------------


CREATE TABLE Comment_On_Message(
  comment_id INTEGER GENERATED ALWAYS AS IDENTITY,
  comment_content VARCHAR2(255) NOT NULL,
  comment_is_read INTEGER NOT NULL,
  comment_sender_id INTEGER NOT NULL,
  comment_message_id INTEGER NOT NULL,
  comment_create_time VARCHAR2(30) NOT NULL,
  CONSTRAINT pk_comment_on_message
	PRIMARY KEY (comment_id),
  CONSTRAINT fk_comment_1 FOREIGN KEY
    (comment_sender_id) REFERENCES
      User_Public_Info(user_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_comment_2 FOREIGN KEY
    (comment_message_id) REFERENCES
      Message(message_id)
        ON DELETE CASCADE
);

CREATE INDEX c_message_id_index ON Comment_On_Message(comment_message_id, comment_create_time);
---------------------------------------------------
---------------------Message_Collection------------
CREATE TABLE Message_Collection(
  user_id INTEGER NOT NULL,
  message_id INTEGER NOT NULL,
  CONSTRAINT pk_m_c PRIMARY KEY (user_id, message_id),
  CONSTRAINT fk_m_c_1 FOREIGN KEY
    (user_id) REFERENCES
      User_Public_Info(user_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_m_c_2 FOREIGN KEY
    (message_id) REFERENCES
      Message(message_id)
        ON DELETE CASCADE
);
---------------------------------------------------
--------------------Topic--------------------------

CREATE TABLE Topic(
  topic_id INTEGER GENERATED ALWAYS AS IDENTITY,
  topic_heat INTEGER NOT NULL,
  topic_content VARCHAR(50) NOT NULL,
  CONSTRAINT pk_topic
	PRIMARY KEY (topic_id)
);

CREATE INDEX t_heat_index ON TOPIC(topic_heat);
----------------------------------------------------
----------------------Message_Owns_Topic------------
CREATE TABLE Message_Owns_Topic(
  message_id INTEGER NOT NULL,
  topic_id INTEGER NOT NULL,
  CONSTRAINT pk_m_o_t PRIMARY KEY (message_id, topic_id),
  CONSTRAINT fk_m_o_t_1 FOREIGN KEY
    (message_id) REFERENCES
      Message(message_id)
        ON DELETE CASCADE,
  CONSTRAINT fk_m_o_t_2 FOREIGN KEY
    (topic_id) REFERENCES
      Topic(topic_id)
        ON DELETE CASCADE
);
