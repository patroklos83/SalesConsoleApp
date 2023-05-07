# .Net Sales [.csv] files statistics exports reader

A .Net console application for importing .csv files consisting of sales records and exporting statistics for standard deviation and averages.

## Case Study

A New agency requires a User friendly Interface, to view, create and edit Articles posted by the agency.


## Requirements

For building the application you need:

- [.Net framework 7 or newer](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Visual Studio IDE

## Running the Application on windows operating system

Step 1. Go to Windows terminal and go to the /bin directory of the application

from the /bin directory run the following command

    SalesConsoleApp.exe

Step 2. Select the .csv files directory where the files to be imported are located. 
*Note: If no directory is specified, the default will be user [bin/ImportFiles]*

Step 3.  Select the format of the dates in the csv files. 
For example a date ***01/01/2001*** has a format of ***dd/MM/yyyy***. 
*If an invalid format is specified then an error will be thrown by the application.*

Step 4. Select the format of the sales Amount. 
For example an Amount ***200.20*** has a format of ***xxxxx.xx***, with no currency symbol as a prefix.

Step 5. Provide a specific date range, provided the user requires to export statistics based on a specific time period.



![enter image description here](/images/Capture1.PNG)
![enter image description here](/images/Capture1.PNG)
![enter image description here](/images/Capture3.PNG)
![enter image description here](/images/Capture4.PNG)



Fell free to grab a copy of this sample code, and play it yourself.
