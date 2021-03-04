-- Create database
CREATE DATABASE g2_integration;

-- Switch to database
use g2_integration;

-- Create API Configuration table
CREATE TABLE api_configurations
(
id VARCHAR(36),
name VARCHAR(20),
credentials_type VARCHAR(20),
credentials VARCHAR(200),
base_url VARCHAR(50),
CONSTRAINT pk_api_configurations PRIMARY KEY (id),
CONSTRAINT uq_api_configurations_name UNIQUE (NAME),
);

-- Create zoom history table
CREATE TABLE zoom_history
(
id VARCHAR(36),
type VARCHAR(20),
start_time DATETIME,
end_time DATETIME,
host VARCHAR(50),
email VARCHAR(254),
user_type VARCHAR(30),
participants INT,
duration VARCHAR(10),
has_pstn VARCHAR(5),
has_voip VARCHAR(5),
has_3rd_party_audio VARCHAR(5),
has_video VARCHAR(5),
has_screen_share VARCHAR(5),
has_recording VARCHAR(5),
has_sip VARCHAR(5),
CONSTRAINT pk_zoom_meetings PRIMARY KEY (id),
CONSTRAINT uq_zoom_meetings_start_date_end_date UNIQUE (end_time, start_time)
);

-- Create G2 report table
CREATE TABLE reports
(
id VARCHAR(36),
type VARCHAR(20),
start_date DATE,
end_date DATE,
status VARCHAR(15),
result TEXT,
CONSTRAINT pk_report PRIMARY KEY (id),
CONSTRAINT uq_report_type_start_date_end_date UNIQUE (type, start_date, end_date, status)
);