
 /*  
 decompress d:\_testout\testTXT.arc  d:\_testout\EntityFrameworkNew.xml  
 compress d:\_testout\EntityFramework.xml  d:\_testout\testTXT.arc

 decompress d:\_testout\testMOV.arc "d:\_testout\Учим CSS Grid за 1 Час!.mp4" 
 compress "d:\_Books\_Video_&_Udemy\Учим CSS Grid за 1 Час!.mp4" d:\_testout\testMOV.arc

 decompress d:\_testout\testMKV.arc  d:\_testout\movie.mkv
 compress "d:\_Books\_Video_&_Udemy\Марафон по верстке сайта. Часть 2.mp4"  d:\_testout\testMKV.arc
 
 decompress d:\_testout\testMKV_1.arc d:\_testout\movie1.mkv
 compress d:\_Films\Отклонение.mkv d:\_testout\testMKV_1.arc
 
 decompress d:\_testout\testBigFile.zip d:\_testout\bigfile.zip
 compress d:\_testout\bigfile.zip  d:\_testout\testBigFile.arc

 */
Compress files 
 
Разработать консольное приложение на C# для поблочного сжатия и распаковки файлов с помощью 
System.IO.Compression.GzipStream. 
Для сжатия исходный файл делится на блоки одинакового размера, например, в 1 мегабайт. 
Каждый блок сжимается и записывается в выходной файл независимо от остальных блоков. 
Программа должна эффективно распараллеливать и синхронизировать обработку блоков в 
многопроцессорной среде и уметь обрабатывать файлы, размер которых превышает объем 
доступной оперативной памяти.  
В случае исключительных ситуаций необходимо проинформировать пользователя понятным 
сообщением, позволяющим пользователю исправить возникшую проблему, в частности, если 
проблемы связаны с ограничениями операционной системы. 
При работе с потоками допускается использовать только базовые классы и объекты синхронизации 
(Thread, Manual/AutoResetEvent, Monitor, Semaphor, Mutex) и не допускается использовать 
async/await, ThreadPool, BackgroundWorker, TPL. 
Код программы должен соответствовать принципам ООП и ООД (читаемость, разбиение на классы 
и т.д.).  
Параметры программы, имена исходного и результирующего файлов должны задаваться в 
командной строке следующим образом: 
GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла] 
В случае успеха программа должна возвращать 0, при ошибке возвращать 1. 
Примечание: формат архива остаётся на усмотрение автора, и не имеет значения для оценки 
качества тестового, в частности соответствие формату GZIP опционально. 
Исходники необходимо прислать вместе с проектом Visual Studio.
