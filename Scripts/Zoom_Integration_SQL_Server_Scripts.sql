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
start_date DATETIME,
end_date DATETIME,
host_name VARCHAR(20),
email VARCHAR(254),
participants INT,
duration VARCHAR(6),
has_pstn BIT,
has_voip BIT,
has_3rd_party_audio BIT,
has_video BIT,
has_screen_share BIT,
has_recording BIT,
has_sip BIT,
CONSTRAINT pk_zoom_meetings PRIMARY KEY (id),
CONSTRAINT uq_zoom_meetings_start_date_end_date UNIQUE (end_date, start_date)
);

-- Create G2 report table
CREATE TABLE report
(
id VARCHAR(36),
type VARCHAR(20),
start_date DATETIME,
end_date DATETIME,
status VARCHAR(10),
result VARCHAR(100),
CONSTRAINT pk_report PRIMARY KEY (id),
CONSTRAINT uq_report_type_start_date_end_date UNIQUE (type, end_date, start_date)
);