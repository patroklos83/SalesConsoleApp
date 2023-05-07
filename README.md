

# .Net Sales [.csv] files statistics exports reader

A .Net console application for importing .csv files consisting of sales records and exporting statistics for standard deviation and averages.

## Case Study

A New agency requires a User friendly Interface, to view, create and edit Articles posted by the agency.


## Requirements

For building the application you need:

- [.Net framework 7 or newer](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Visual Studio IDE

## Running the Application on windows operating system

The import .csv files has to be ordered based on the dates included, so the application can read them in sorted way.

*see below example of 2 import files**

Sales1.csv

       31/03/2020##1245.39 
       31/03/2020##245.39 
       31/03/2020##245.39   
       01/04/2020##345.39 
       01/05/2021##245.39 
       02/05/2021##229.39   
       03/05/2021##340.39

Sales2.csv

    03/06/2021##1245.39
    04/06/2021##243.39
    31/03/2022##24.39
    01/04/2022##345.39
    01/05/2023##25.39
    02/05/2023##25.00
    03/05/2024##34.00

Step 1. Go to Windows terminal and go to the /bin directory of the application

from the /bin directory run the following command

    SalesConsoleApp.exe

Step 2. Select the .csv files directory where the files to be imported are located. 
*Note: If no directory is specified, the default will be user [bin/ImportFiles]*

Step 3.  Select the format of the dates in the csv files. 
For example a date ***01/01/2001*** has a format of ***dd/MM/yyyy***. 
*If an invalid format is specified then an error will be thrown by the application.*

![enter image description here](/images/Capture1.PNG)

Step 4. Select the format of the sales Amount. 
For example an Amount ***200.20*** has a format of ***xxxxx.xx***, with no currency symbol as a prefix.

![enter image description here](/images/Capture2.PNG)

Step 5. Provide a specific date range, provided the user requires to export statistics based on a specific time period.

![enter image description here](/images/Capture3.PNG)

See the Statistics !

![enter image description here](/images/Capture4.PNG)

![enter image description here](/images/Capture5.PNG)

Fell free to grab a copy of this sample code, and play it yourself.
