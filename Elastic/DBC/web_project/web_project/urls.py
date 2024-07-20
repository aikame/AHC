"""
URL configuration for web_project project.

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/4.2/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.contrib.auth.models import User
from django.urls import path,include
from rest_framework import routers, serializers, viewsets
from SCTDB import views

class UserSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = User
        fields = ['url','username','email','is_staff']

class UserViewSet(viewsets.ModelViewSet):
    queryset = User.objects.all()
    serializer_class = UserSerializer

router = routers.DefaultRouter()
router.register(r'users',UserViewSet)


urlpatterns = [
    path('', include(router.urls)),
    path('api-auth/', include('rest_framework.urls')),
    path('api/put', views.profile_detail),
    path('api/getall', views.profile_detail),
    path('api/getone',views.get_one),
    path("api/img_upload/<str:id>", views.emp_avatar_upload),
    path('api/get',views.get_text),
    path('api/add_to_profiles', views.add_to_profiles, name='add_to_profiles'),
    path('api/fire_user', views.fire_user),
    path('api/return_user', views.return_user),
    path("api/ComputerData",views.computer_data),
    path("api/GetComputer",views.get_computer_data)
]
