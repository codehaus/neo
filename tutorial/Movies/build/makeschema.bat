neo.exe /t NeoCreateDb.vtl /o %TEMP%\movies-schema.sql ../src/Model/movies-schema.xml
osql -S VIRTSERVER -U dev -P passw0rd -d Movies -i %TEMP%\movies-schema.sql -n

