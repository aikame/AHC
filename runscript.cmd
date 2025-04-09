@echo off
echo Starting Frontend...
cd Frontend
start "Frontend" cmd /k ".\.venv\Scripts\python.exe manage.py runserver"
cd ..

echo Starting DBC...
cd Elastic\DBC
start "DBC" cmd /k ".\.venv\Scripts\python.exe web_project\manage.py runserver 127.0.0.2:8000"