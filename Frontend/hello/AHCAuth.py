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

class AHCAuthBackend(BaseBackend):
     def authenticate(self, username=None, password=None):
        rawdata = '{"user":"' +username+ '","password":"'+password+'"}'
        response = requests.post('https://localhost:7095/Authentication',data=rawdata,headers={"Content-Type":"application/json"},verify=False)
        if response.status_code == 200:
            user, created = User.objects.get_or_create(username=username)
            if created:
                user.set_password(password)
                user.save()
            return user,True
        return None,False