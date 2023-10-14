from django.urls import path, include
from hello import views

urlpatterns = [
    path("", views.home, name="home"),
    path("settings", views.settings, name="settings"),
    path("hello/<name>", views.hello_there, name="hello_there"),
    path('api-auth/', include('rest_framework.urls')),
    
]