from django.urls import path
from django import include
from hello import views

urlpatterns = [
    path("", views.home, name="home"),
    path("hello/<name>", views.hello_there, name="hello_there"),
    path('api-auth/', include('rest_framework.urls'))
]