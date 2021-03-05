# G2_Take_Home_Assignment

Problem statement:
Your business stakeholder has tasked you and your team with building an application (a reporting
app) that works off the Zoom API. The requirement is for you to present Zoom usage data over the
last 90 days. You need to do this in such a way that the reporting application auto-updates
everyday with Zoom usage over a rolling 90 day window. Along with the usage your stakeholder
would also want a report about the opportunities where spend can be optimized on Zoom licenses.

THE ASK:
Based on this requirement, design a schema with appropriate simple visualizations. What factors
will you take into account when designing this integration?

BONUS QUESTIONS
1.How would you go about generalising your solution in a way where it could be applicable to
another API?
2. How would you design this to be as plug & play as possible for an end-user?
3. How would you design this application to be as robust as possible?

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
