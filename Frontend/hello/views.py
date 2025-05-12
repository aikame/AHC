from datetime import datetime
from django.contrib.auth.base_user import AbstractBaseUser
from django.http import JsonResponse, HttpResponse, HttpRequest
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
from django.conf import settings
from .AHCAuth import AHCAuthBackend
from .forms import UploadFileForm
import os
from django.utils.dateparse import parse_datetime
from django import template
from django.utils import timezone
from django.http import HttpResponseRedirect
from django.urls import reverse
from rest_framework.response import Response
from rest_framework import status
from rest_framework.renderers import JSONRenderer
import asyncio
import aiohttp
import re
import urllib.parse
from django.utils.timezone import make_aware,get_current_timezone, utc
import pytz
register = template.Library()

ROLE_CHOICES = {
    0: "Standalone Workstation",
    1: "Member Workstation",
    2: "Standalone Server",
    3: "Member Server",
    4: "Backup Domain Controller (BDC)",
    5: "Primary Domain Controller"
}

# Высрал здесь роли на будущее

USER_ROLE = {
    "AHC_ADM": "AHC-Administrator",
    "SECU": "IT-Security",
    "ADM": "Administrator",
    "1CADM": "1C-Administrator",
}
timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
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
    user = user.replace('\\', '\\\\')
    print("Creds: " + user + ' ' + password)
    aBack = AHCAuthBackend()
    user,status = aBack.authenticate(username=user,password=password)
    if (status):
        login(request, user)
        return render(
            request,
            'main_page/index.html'
        )
    else:
        return render(
        request,
        'login/index.html',
        {'errortxt':"Неправильное имя пользователя или пароль"}
        )

@login_required
def home(request):
    return render( 
        request,
        'main_page/index.html',
    )
@login_required
def preferences(request):
    domains = requests.get('https://localhost:7095/domainList/',verify=False)
    return render(
        request,
        'settings/index.html',
        {'domains':json.loads(domains.content)}
    )
    
@login_required
def img_upload(request, id):
    if request.method == 'POST':
        form = UploadFileForm(request.POST, request.FILES)
        if form.is_valid():
            avatar = form.cleaned_data['avatar']
            avatar_path = os.path.join(settings.MEDIA_ROOT, 'employee_avatars', f'{id}.jpg')
            with open(avatar_path, 'wb+') as destination:
                for chunk in avatar.chunks():
                    destination.write(chunk)
            #data = '{"doc": {"img_src" : "' + id +'.jpg"} }'
            #content = JSONRenderer().render(data)

            profile = requests.get('https://localhost:7080/search/oneprofile?query='+id,verify=False)
            profileJSON = json.loads(profile.content)
            profileJSON["imgSrc"]=id+'.jpg'
            print(profileJSON)
            content = JSONRenderer().render(profileJSON)
            req = requests.post('https://localhost:7080/profile/update', data=content, verify=False,headers={"Content-Type": "application/json"})
            print(req)
            return redirect(f'/employee/{id}')
    else:
        form = UploadFileForm()
    return render(request, 
                  'img_upload/index.html')

@login_required
def employee(request,id):
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
    user_data = requests.get('https://localhost:7080/search/oneprofile?query='+id,verify=False)
    domains_data = requests.get('https://localhost:7095/domainList/',verify=False)
    domains = json.loads(domains_data.content)
    domainsList = list(domains)
    data = json.loads(user_data.content)
    user = data
    user["applyDate"] = parse_datetime(user['applyDate'])
    print(user["fireDate"])
    if (user["fireDate"] != None):
        user["fireDate"] = parse_datetime(user['fireDate'])
    for profile in user["profiles"]:
        if "AD" in profile:
            for domain in domains:
                if profile["AD"]["domain"] == domain:
                    domainsList.remove(domain)
    domains = json.dumps(domainsList)
    return render(
        request,
        'employee/index.html',
        {'profile_json':user, 'domains':domainsList,"id":id}
    )
@login_required
def computer_detail(request, id):
    computer_data = requests.get(f'https://localhost:7080/search/onecomputer?query={id}',verify=False) 
    data = json.loads(computer_data.content)
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
    apps = None  # Если не достучались до пк
    try:
        installed_apps = requests.get(
            f'https://localhost:7095/GetAppInfo?Computer={data["computerName"]}&domain={data["domainName"]}', 
            verify=False,
            timeout=30 
        )
        installed_apps.raise_for_status() 
        apps = json.loads(installed_apps.content).get("AppList", [])

        for app in apps:
            app["DisplayVersion"] = app.get("DisplayVersion") or ""
            app["Publisher"] = app.get("Publisher") or ""
            install_date = app.get("InstallDate")
            if install_date and len(install_date) == 8:
                app["InstallDate"] = f"{install_date[6:8]}.{install_date[4:6]}.{install_date[:4]}"
            else:
                app["InstallDate"] = ""

    except (requests.RequestException, json.JSONDecodeError) as e:
        print(f"Ошибка: {e}") 

    role_number = data.get('computerRole', -1)
    now = timezone.now()
    updated_time = timezone.datetime.fromisoformat(data["updated"].replace('Z', '+00:00'))
    last_upd = int((now - updated_time).total_seconds() // 60)
    data['updated'] = parse_datetime(data['updated'])
    data["computerRole"] = ROLE_CHOICES.get(role_number, "Unknown Role")

    return render(
        request,
        'computer_detail/index.html',
        {
            'computer_json': data,
            'difMinutes': last_upd,
            'id': id,
            "apps": apps  # None если не достучались до пк
        }
    )

@login_required
def update_computer_status(request,id):
    update = requests.get('https://localhost:7095/CheckComputer?_id='+id,verify=False)

    return HttpResponseRedirect(reverse('computer', args=[id]))
@login_required
def computer(request):
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
    json_data = requests.get('https://localhost:7080/search/computer', verify=False)
    data = json.loads(json_data.content)
    for i in data:        
        role_number = i.get('computerRole', -1)
        i["computerRole"] = ROLE_CHOICES.get(role_number, "Unknown Role")
        i['updated'] = parse_datetime(i['updated'])

    return render(
        request,
        'computer/index.html',
        {'computers_json':data}
    )

@login_required
def groups(request):
    json_data = requests.get('https://localhost:7080/search/group',verify=False)
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
    try:
        data = json.loads(json_data.content)
    except:
        data = 0;
    return render(
        request,
        'groups/index.html',
        {'groups_json':data}
    )


@login_required
def group_detail(request, id):
    group_data = requests.get(f'https://localhost:7080/search/onegroup?query={id}',verify=False)
    data = json.loads(group_data.content)
    print(data)
    group = data
    members = None
    try:
        dn = group.get("distinguishedName", "")

        dc_parts = re.findall(r"DC=([^,]+)", dn)

        domain = ".".join(dc_parts)
        group_name_encoded = urllib.parse.quote(group["name"]).replace("%20", "+")
        print(domain) 
        getMembersReq = requests.get(f'https://127.0.0.1:7095/GetGroupMembers?group={group_name_encoded}&domain={domain}',verify = False, timeout=30)
        getMembersReq.raise_for_status()
        members_data= json.loads(getMembersReq.content).get("Members")
        if isinstance(members_data, dict):
            members = [members_data]
        else:
            members = members_data
        for mem in members:
            mem["extensionAttribute1"] = mem.get('extensionAttribute1') or "" #################################!!!!!!!!!!!!!!!!
            mem["extensionAttribute2"] = mem.get('extensionAttribute2') or ""
            mem["extensionAttribute3"] = mem.get('extensionAttribute3') or ""
            mem["Title"] = mem.get('Title') or ""
        print(members)
    except (requests.RequestException, json.JSONDecodeError) as e:
        print(f"Ошибка: {e}")  
    return render(
        request,
        'group/index.html',
        {
            'group_json': data,
            'id': id,
            'members':members, # =null
            'domain':domain
        }
    )

@login_required
def searchall(request):
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
    json_data = requests.get('https://localhost:7080/search/profile',verify=False)
    try:
        data = json.loads(json_data.content)
    except:
        data = 0
    for i in data:
        if ("fireDate" in i and i["fireDate"] != None):
            i["fireDate"] = parse_datetime(i['fireDate'])    
            print(i["fireDate"])
    return render(
        request,
        'profileslist/index.html',
        {'profiles_json':data}
    )

@login_required
def createAD(request,id,domain):
    print(domain)
    user_data = requests.get(f'https://localhost:7080/search/oneprofile?query={id}',verify=False)
    mail = request.GET.get('mail')
    print(mail)
    user = user_data.json()
    content = JSONRenderer().render(user)
    result = requests.post(f'https://localhost:7095/CreateUser?domain={domain}&mail={mail}',data=content,verify=False,headers={"Content-Type": "application/json"})
    if result.status_code == 200:
        return JsonResponse(result.json(), status=200)
    else:
        return JsonResponse({'error': 'Profile not updated successfully'}, status=500)

@login_required
def search(request,location,text):
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))
    print('"'+location+"/"+text+'"')
    if (text == ""):
        text = "*"
    json_data = requests.get(f'https://localhost:7080/search/{location}?query={text}',headers={"Content-Type":"application/json"}, verify=False)
    data = json.loads(json_data.content)
    for i in data:
        if ("fireDate" in i and i["fireDate"] != None):
            i["fireDate"] = parse_datetime(i['fireDate'])    
            print(i["fireDate"])
    renderurl = ""
    rendername = ""
    if (location == "profile"):
        renderurl = 'profileslist/index.html'
        rendername = 'profiles_json'
    elif (location == "computer"):
        renderurl = 'computer/index.html'
        rendername = 'computers_json'
    else:
        renderurl = 'groups/index.html'
        rendername = 'groups_json'
    return render(
        request,
        renderurl,
        {rendername:data}
    )
def sync_render_ad(request, domain, id):
    return asyncio.run(active_directory_async(request, domain, id))

@login_required
def active_directory(request, domain, id):
    return sync_render_ad(request, domain, id)

async def active_directory_async(request, domain, id):
    print(id)
    timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))

    clean_domain = domain.rsplit(".", 1)[0] if domain.endswith((".com", ".ru")) else domain

    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(verify_ssl=False)) as session:
        urls = {
            'user_data': f'https://localhost:7095/GetInfo?id={id}&domain={domain}',
            'groups_req': f'https://localhost:7080/search/group?query={clean_domain}',
            'profile_data': f'https://localhost:7080/search/oneprofile?query={id}',
        }

        async def fetch_json(name, url):
            async with session.get(url, headers={"Content-Type": "application/json"}) as resp:
                content = await resp.read()
                try:
                    return name, json.loads(content)
                except (json.JSONDecodeError, TypeError):
                    return name, None

        results = await asyncio.gather(*(fetch_json(k, v) for k, v in urls.items()))
        result_dict = dict(results)

    return render(
        request,
        "active_directory/index.html",
        {
            'id': id,
            'ad_json': result_dict['user_data'],
            'domain': domain,
            'groups': result_dict['groups_req'],
            'profile': result_dict['profile_data']
        }
    )

@login_required
def create_group(request):
    #user_data = requests.get('http://127.0.0.2:8000/api/getone',data='{"id":"'+id+'"}')
    #mail = request.GET.get('mail')
    if request.method == 'POST':
        data = {
            'Name': request.POST.get('Name'),
            'Description': request.POST.get('Description'),
            'domain': request.POST.get('Domain'),
        }
        print(data)
        user = json.dumps(data)
        print(user)
        result = requests.post(f'https://localhost:7095/CreateGroup',data=user,verify=False,headers={"Content-Type": "application/json"})
        print(result.text)
        if result.status_code ==200:
            return redirect("/settings")
        else:
            return JsonResponse({'error': 'grupu not created successfully'}, status=500)
        
    else:
        return JsonResponse({'success': 'Invalid request'}, status=400)

@login_required
def create_profile(request):
    #user_data = requests.get('http://127.0.0.2:8000/api/getone',data='{"id":"'+id+'"}')
    #mail = request.GET.get('mail')
    if request.method == 'POST':
        data = {
            'Name': request.POST.get('name'),
            'Surname': request.POST.get('surname'),
            'Patronymic': request.POST.get('patronymic'),
            'Company': request.POST.get('company'),
            'ApplyDate': request.POST.get('apply_date'),
            'Appointment': request.POST.get('appointment'),
            'City': request.POST.get('city'),
            'ADreq': False
        }
        print(data)
        user = json.dumps(data)
        print(user)
        result = requests.post(f'https://localhost:7095/CreateProfile',data=user,verify=False,headers={"Content-Type": "application/json"})
        print(result.text)
        if result.status_code ==200:
            return JsonResponse({'success': 'Profile created successfully'}, status=200)
        else:
            return JsonResponse({'error': 'Profile not created successfully'}, status=500)
        
    else:
        return JsonResponse({'success': 'Invalid request'}, status=400)

    path("showMail",view=showMail,name="showMail"),
    path("hideMail",view=hideMail,name="hideMail"),
    path("ban",view=ban,name="ban"),
    path("unban",view=unban,name="unban"),
    path("addToGroup",view=addToGroup,name="addToGroup"),
    path("removeFromGroup",view=removeFromGroup,name="removeFromGroup"),
    path("ChangePassword",view=ChangePassword,name="ChangePassword"),

@login_required
def showMail(request,domain,id):
    result = requests.get(f'https://localhost:7095/ShowMailBox?domain={domain}&id={id}',verify=False)
    if result.status_code ==200:
        return JsonResponse({'success': 'showMail successfull'}, status=200)
    else:
        return JsonResponse({'error': 'showMail unsuccessfull'}, status=500)


@login_required
def hideMail(request,domain,id):
    result = requests.get(f'https://localhost:7095/HideMailBox?domain={domain}&id={id}',verify=False)
    if result.status_code ==200:
        return JsonResponse({'success': 'hideMail successfull'}, status=200)
    else:
        return JsonResponse({'error': 'hideMail unsuccessfull'}, status=500)
@login_required
def createMail(request,domain,id):
    result = requests.get(f'https://localhost:7095/CreateMailBox?domain={domain}&id={id}',verify=False)
    if result.status_code ==200:
        return JsonResponse({'success': 'CreateMailBox successfull'}, status=200)
    else:
        return JsonResponse({'error': 'CreateMailBox unsuccessfull'}, status=500)

@login_required
def ban(request,domain,id):
    result = requests.get(f'https://localhost:7095/BanUser?id={id}&domain={domain}',verify=False)
    if result.status_code ==200:
        return JsonResponse({'success': 'ban successfull'}, status=200)
    else:
        return JsonResponse({'error': 'ban unsuccessfull'}, status=500)

@login_required
def unban(request,domain,id):
    print(id)
    print(domain)
    result = requests.get(f'https://localhost:7095/UnbanUser?id={id}&domain={domain}',verify=False)
    if result.status_code ==200:
        return JsonResponse({'success': 'unban successfull'}, status=200)
    else:
        return JsonResponse({'error': 'unban unsuccessfull'}, status=500)

@login_required
def addToGroup(request,domain,id):
    result = requests.post(f'https://localhost:7095/AddToGroup',data=request.body,verify=False,headers={"Content-Type": "application/json"})
    if result.status_code ==200:
        return JsonResponse({'success': 'addToGroup successfull'}, status=200)
    else:
        return JsonResponse({'error': 'addToGroup unsuccessfull'}, status=500)

@login_required
def removeFromGroup(request):
    result = requests.post(f'https://localhost:7095/RemoveFromGroup',data=request.body,verify=False,headers={"Content-Type": "application/json"})
    print(result.text)
    if result.status_code ==200:
        return JsonResponse({'success': 'addToGroup successfull'}, status=200)
    else:
        return JsonResponse({'error': 'addToGroup unsuccessfull'}, status=500)

@login_required
def changePassword(request):
    result = request.post(f'https://localhost:7095/ChangePassword', data=request.body,verify=False,headers={"Content-Type": "application/json"})
    print(result.json())
    if result.status == 200:
        return JsonResponse(result.json(), status = 200)
    else:
        return JsonResponse({'error': 'Что-то пошло не так'})

timezone.activate(pytz.timezone('Asia/Krasnoyarsk'))