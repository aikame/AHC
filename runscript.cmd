@echo off
echo Starting Frontend...
cd Frontend
start "Frontend" cmd /k ".\.venv\Scripts\python.exe manage.py runserver"
cd ..
