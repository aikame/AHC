from django.contrib.auth.base_user import AbstractBaseUser
from django.http import HttpResponse, HttpRequest
from django.shortcuts import render, redirect
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

from django.utils.dateparse import parse_datetime
from django import template
from django.utils import timezone
register = template.Library()

ROLE_CHOICES = {
    0: "Standalone Workstation",
    1: "Member Workstation",
    2: "Standalone Server",
    3: "Member Server",
    4: "Backup Domain Controller (BDC)",
    5: "Primary Domain Controller"
}


@register.filter
def get_role_display(role_number):
    return ROLE_CHOICES.get(role_number, "Unknown Role")
        

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
    domains = requests.get('https://localhost:7095/domainList/',verify=False)
    return render(
        request,
        'settings/index.html',
        {'domains':json.loads(domains.content)}
    )
    
@login_required
def img_upload(request, id):
    user_id = id
    if request.method == 'POST':
        form = UploadFileForm(request.POST, request.FILES)
        if form.is_valid():
            avatar_inst, created = EMPImage.objects.get_or_create(user_id=user_id)
            avatar_inst.avatar = form.cleaned_data['avatar']
            avatar_inst.save()
            return redirect('search')
    else:
        form = UploadFileForm()
    return render(request, 
                  'img_upload/index.html')

@login_required
def employee(request,id):
    user_data = requests.get('http://127.0.0.2:8000/api/getone',data='{"id":"'+id+'"}')
    domains_data = requests.get('https://localhost:7095/domainList/',verify=False)
    domains = json.loads(domains_data.content)
    domainsList = list(domains)
    data = json.loads(user_data.content)
    user = data["hits"]["hits"][0]["_source"]
    user_id = data["hits"]["hits"][0]["_id"]
    for profile in user["profiles"]:
        if "AD" in profile:
            for domain in domains:
                if profile["AD"]["domain"] == domain:
                    domainsList.remove(domain)
    domains = json.dumps(domainsList)
    return render(
        request,
        'employee/index.html',
        {'profile_json':user, 'domains':domainsList, 'user_id': user_id}
    )
@login_required
def computer_detail(request,id):
    computer_data = requests.get('http://127.0.0.2:8000/api/GetComputer?_id='+id)
    data = json.loads(computer_data.content)
    role_number = data.get('ComputerRole', -1)
    now = timezone.now()
    time = parse_datetime(data['updated'])
    updated_time = timezone.datetime.fromisoformat(data["updated"].replace('Z', '+00:00'))
    difTime =  now - updated_time
    last_upd = int(difTime.total_seconds() // 60)
    data['updated'] = time  #parse_datetime(data['updated'])
    data["ComputerRole"] = ROLE_CHOICES.get(role_number, "Unknown Role")
    return render(
        request,
        'computer_detail/index.html',
        {'computer_json':data,
         'difMinutes':last_upd
         }
    )
@login_required
def computer(request):
    json_data = requests.get('http://127.0.0.2:8000/api/ComputerData')
    data = json.loads(json_data.content)

    for i in data["hits"]["hits"]:
        role_number = i['_source'].get('ComputerRole', -1)
        i['_source']["ComputerRole"] = ROLE_CHOICES.get(role_number, "Unknown Role")
        i['_source']['updated'] = parse_datetime(i['_source']['updated'])
        i['source'] = i.pop('_source')
        i['id'] = i.pop('_id')
    return render(
        request,
        'computer/index.html',
        {'computers_json':data["hits"]["hits"]}
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
    domain_data = requests.get(f'http://127.0.0.2:8000/api/GetComputer?domain={domain}')
    json_domain = json.loads(domain_data.content)
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