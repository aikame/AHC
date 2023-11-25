from django.http import HttpResponse, HttpRequest
from django.shortcuts import render
import requests 
import json

def home(request):
    return render(
        request,
        'main_page/index.html',
    )

def settings(request):
    return render(
        request,
        'settings/index.html',
    )
def employee(request,id):
    user_data = requests.get('http://127.0.0.2:8000/api/getone',data='{"id":"'+id+'"}')
    data = json.loads(user_data.content)
    return render(
        request,
        'employee/index.html',
        {'profile_json':data["hits"]["hits"][0]["_source"]}
    )
def computer(request):
    return render(
        request,
        'computer/index.html',
    )
def searchall(request):
    json_data = requests.get('http://127.0.0.2:8000/api/getall')
    data = json.loads(json_data.content)
    for i in data["hits"]["hits"]:
        i['source'] = i.pop('_source')
        i['id'] = i.pop('_id')
    return render(
        request,
        'profileslist/index.html',
        {'profiles_json':data["hits"]["hits"]}
    )
def search(request,text):
    print('"'+text+'"')
    if (text == ""):
        text = "*"
    search_text = '{"text":"'+text+'"}'
    json_data = requests.get('http://127.0.0.2:8000/api/get',data=search_text.encode('utf-8'),headers={"Content-Type":"application/json"})
    data = json.loads(json_data.content)
    for i in data["hits"]["hits"]:
        i['source'] = i.pop('_source')
        i['id'] = i.pop('_id')
    return render(
        request,
        'profileslist/index.html',
        {'profiles_json':data["hits"]["hits"]}
    )
def login(request):
    return render(
        request,
        'login/index.html',
    )
def active_directory(request,id):
    return render(
        request,
        "active_directory/index.html",
        {'id':id},
    )