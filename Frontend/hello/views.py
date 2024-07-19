from django.contrib.auth.base_user import AbstractBaseUser
from django.http import HttpResponse, HttpRequest
from django.shortcuts import render
from django.contrib.auth.decorators import login_required
from django.contrib.auth import authenticate, login, logout
from django.contrib.auth.backends import BaseBackend
from django.contrib.auth.models import User
from django.contrib import messages
#from rest_framework.renderers import JSONRenderer
from django.views.decorators.csrf import csrf_protect
import requests 
import json
from .AHCAuth import AHCAuthBackend
from .forms import UploadFileForm
from .EmployeeAvatar import upload_emp_avatar


        

@csrf_protect
def login_page(request):
    return render(
        request,
        'login/index.html',
    )

@csrf_protect
def auth(request):
    user = request.POST.get('user')
    password = request.POST.get('password')
    print("Creds: " + user + ' ' + password)
    aBack = AHCAuthBackend()
    user = aBack.authenticate(username=user,password=password)
    if (user != 200):
        login(request, user)    
        return render(
            request,
            'main_page/index.html'
        )
    else:
        return render(
            request,
            'login/index.html',
        )

@login_required
def home(request):
    return render( 
        request,
        'main_page/index.html',
    )
@login_required
def settings(request):
    return render(
        request,
        'settings/index.html',
    )
    
@login_required
def img_upload(request, id):
    if request.method == 'POST':
        form = UploadFileForm(request.POST, request.FILES)
        if form.is_valid():
            upload_emp_avatar(request.FILES["file"])
    user_data = requests.get('http://127.0.0.2:8000/api/getone',data='{"id":"'+id+'"}')
    data = json.loads(user_data.content)
    return render(
        request,
        'employee/index.html',
        {'profile_json':data["hits"]["hits"][0]["_source"]}
    )

@login_required
def employee(request,id):
    user_data = requests.get('http://127.0.0.2:8000/api/getone',data='{"id":"'+id+'"}')
    data = json.loads(user_data.content)
    return render(
        request,
        'employee/index.html',
        {'profile_json':data["hits"]["hits"][0]["_source"]}
    )
@login_required
def computer(request):
    return render(
        request,
        'computer/index.html',
    )
@login_required
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
@login_required
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
@login_required
def active_directory(request,domain,id):
    print(id)
    user_data = requests.get('https://localhost:7095/GetInfo?id='+id+"&domain="+domain,verify=False)
    #domain_data = requests.get(f'http://127.0.0.2:8000/api/GetComputer?domain={domain}')
    #json_domain = json.loads(domain_data.content)
    data = json.loads(user_data.content)
    return render(
        request,
        "active_directory/index.html",
        {
            'id':id,
            'ad_json':data,
            'domain':domain,
        }

    )