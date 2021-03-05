# G2_Take_Home_Assignment

Propose a solution for a stakeholder who wants to view their Zoom usage and optimization reports for the last 90 days and 60 days respectively.

Solution:
1. For usage report:
Create a worker service that runs every day at midnight and processes the reports for the last 90 days. 
It will check the G2 database for relevant data and fetch the remaining data from Zoom.

2. For optimization report:
The same worker service will run a separate timer to process the optimization reports for the last 60 days. 
It will fetch data from the G2 database process the report.

There will be an API exposed to view the reports and status of the reports(SUCCESS, IN_PROGRESS OR ERROR).
Incase there are no reports found, it gives a similar response back.

The data store used for the assignment is MS SQL Server.
