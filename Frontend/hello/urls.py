from django.urls import path, include
from hello import views

urlpatterns = [
    path("", views.home, name="home"),
    path("settings", views.settings, name="settings"),
    path("employee", views.employee, name="employee"),
    path("computer", views.computer, name="computer"),
    path("search", views.search, name="search"),
    path("login", views.login, name="login"),
]