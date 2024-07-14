from django.shortcuts import render
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework import status
from rest_framework.decorators import api_view
from rest_framework.renderers import JSONRenderer
from SCTDB.models import Profile,Computer
from SCTDB.serializers import ProfileSerializer,IdSerializer,ComputerSerializer
import json
import requests
# Create your views here.

@api_view(['POST','GET'])
def profile_detail(request):
    print(request.data)
    try:
        profile = Profile.objects
    except Profile.DoesNotExist:
        return Response(status=status.HTTP_404_NOT_FOUND)
    
    if request.method == 'POST':
        if request.data['fire_date'] == '':
            request.data['fire_date'] = None
        serializer = ProfileSerializer(data=request.data)
        print(serializer.is_valid())
        if serializer.is_valid():
            content = JSONRenderer().render(serializer.data)
            response = requests.post('http://localhost:9200/users/_doc',data=content,headers={"Content-Type":"application/json"})
            return Response(response.content)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    elif request.method == 'GET':
        response = requests.get('http://localhost:9200/users/_search')
        return Response(response.json())
    
@api_view(['POST','GET'])
def computer_data(request):
    print(request.data)
    try:
        computer = Computer.objects
    except Computer.DoesNotExist:
        return Response(status=status.HTTP_404_NOT_FOUND)
    
    if request.method == 'POST':
        serializer = ComputerSerializer(data=request.data)
        print(serializer.is_valid())
        if serializer.is_valid():
            computerName = request.data['ComputerName']
            response = requests.get("http://localhost:9200/computers/_search", data='{"query": {"simple_query_string": {"query": "'+ computerName +'"}}}',headers={"Content-Type":"application/json"})
            search_results = response.json()
            print(search_results)
            if 'hits' in search_results and search_results['hits']['hits']:
                existing_computer_data = search_results['hits']['hits'][0]['_source']
                id = search_results['hits']['hits'][0]['_id']
                existing_computer_data['WindowsEdition'] = request.data['WindowsEdition']
                existing_computer_data['IPAddress'] = request.data['IPAddress']
                existing_computer_data['DomainName'] = request.data['DomainName']
                existing_computer_data['TotalRAMGB'] = request.data['TotalRAMGB']
                existing_computer_data['DiskSpace'] = request.data['DiskSpace']
                existing_computer_data['CPUName'] = request.data['CPUName']
                existing_computer_data['CPUCores'] = request.data['CPUCores']
                existing_computer_data['ComputerName'] = request.data['ComputerName']
                existing_computer_data['Status'] = request.data['ComputerName']
                print(f"update {computerName}")
                response = requests.put(f"http://localhost:9200/computers/_doc/{id}", headers={"Content-Type": "application/json"}, json=existing_computer_data)
            else:
                content = JSONRenderer().render(serializer.data)
                print(f"creating {computerName}")
                response = requests.post('http://localhost:9200/computers/_doc',data=content,headers={"Content-Type":"application/json"})               

            return Response(response.content)
        print(serializer.errors)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    elif request.method == 'GET':
        response = requests.get('http://localhost:9200/computers/_search')
        return Response(response.json())

@api_view(['POST'])
def fire_user(request):
    data = json.loads(request.body)
    profile_id = data.get('id')
    fire_date = data.get('fire_date')
    if not profile_id:
        return Response({'error': '_id is required'}, status=status.HTTP_400_BAD_REQUEST)
    print(2)
    if not fire_date:
        return Response({'error': 'fire_date data is required'}, status=status.HTTP_400_BAD_REQUEST)
    print(3)
    search_url = 'http://localhost:9200/users/_search'
    query = {
        "query": {
            "term": {
                "_id": profile_id
            }
        }
    }
    response = requests.get(search_url, headers={"Content-Type": "application/json"}, json=query)
    search_results = response.json()
    
    if not search_results['hits']['hits']:
        return Response({'error': 'Profile not found'}, status=status.HTTP_404_NOT_FOUND)
    
    existing_profile_data = search_results['hits']['hits'][0]['_source']


    existing_profile_data['fire_date'] = fire_date
    update_url = f'http://localhost:9200/users/_doc/{profile_id}'
    response = requests.put(update_url, headers={"Content-Type": "application/json"}, json=existing_profile_data)
    
    if response.status_code == 200:
        return Response({'success': 'Profile updated successfully'}, status=status.HTTP_200_OK)
    else:
        return Response({'error': 'Failed to update profile'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

@api_view(['POST'])
def return_user(request):
    data = json.loads(request.body)
    profile_id = data.get('id')
    if not profile_id:
        return Response({'error': '_id is required'}, status=status.HTTP_400_BAD_REQUEST)
    
    search_url = 'http://localhost:9200/users/_search'
    query = {
        "query": {
            "term": {
                "_id": profile_id
            }
        }
    }
    response = requests.get(search_url, headers={"Content-Type": "application/json"}, json=query)
    search_results = response.json()
    
    if not search_results['hits']['hits']:
        return Response({'error': 'Profile not found'}, status=status.HTTP_404_NOT_FOUND)
    
    existing_profile_data = search_results['hits']['hits'][0]['_source']


    existing_profile_data['fire_date'] = None
    print(existing_profile_data)
    update_url = f'http://localhost:9200/users/_doc/{profile_id}'
    response = requests.put(update_url, headers={"Content-Type": "application/json"}, json=existing_profile_data)
    print(response.content)
    if response.status_code == 200:
        return Response({'success': 'Profile updated successfully'}, status=status.HTTP_200_OK)
    else:
        return Response({'error': 'Failed to update profile'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

@api_view(['POST'])
def add_to_profiles(request):
    # Получение данных из тела запроса
    data = json.loads(request.body)
    profile_id = data.get('_id')
    profile_data = data.get('profile')
    mail = data.get("email")
    print(profile_data)
    if not profile_id:
        return Response({'error': '_id is required'}, status=status.HTTP_400_BAD_REQUEST)
    
    if not profile_data:
        return Response({'error': 'profile data is required'}, status=status.HTTP_400_BAD_REQUEST)
    
    # Поиск профиля в Elasticsearch по _id
    search_url = 'http://localhost:9200/users/_search'
    query = {
        "query": {
            "term": {
                "_id": profile_id
            }
        }
    }
    response = requests.get(search_url, headers={"Content-Type": "application/json"}, json=query)
    search_results = response.json()
    
    if not search_results['hits']['hits']:
        return Response({'error': 'Profile not found'}, status=status.HTTP_404_NOT_FOUND)
    
    existing_profile_data = search_results['hits']['hits'][0]['_source']
    existing_profile_data['email'] = mail
    # Добавление нового значения в список profiles
    if 'profiles' in existing_profile_data:
        existing_profile_data['profiles'].append(profile_data)
    else:
        existing_profile_data['profiles'] = [profile_data]
    
    # Обновление профиля в Elasticsearch
    update_url = f'http://localhost:9200/users/_doc/{profile_id}'
    response = requests.put(update_url, headers={"Content-Type": "application/json"}, json=existing_profile_data)
    
    if response.status_code == 200:
        return Response({'success': 'Profile updated successfully'}, status=status.HTTP_200_OK)
    else:
        return Response({'error': 'Failed to update profile'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

@api_view(['GET','DELETE', 'POST'])
def get_one(request):
    if request.method == 'GET':
        data = json.loads(request.body)
        response = requests.get("http://localhost:9200/users/_search", data='{"query": {"term": {"_id": "'+ data["id"]+'"}}}',headers={"Content-Type":"application/json"})
        return Response(response.json())
    elif request.method == 'DELETE':
        data = json.loads(request.body)
        response = requests.delete(f"http://localhost:9200/users/_doc/{data['id']}",headers={"Content-Type":"application/json"})
        return Response(response.json())
    elif request.method == 'POST':
        print(request.body)
        data = json.loads(request.body)
        response = requests.get("http://localhost:9200/users/_search", data='{"query": {"term": {"_id": "'+ data["id"]+'"}}}',headers={"Content-Type":"application/json"})
        return Response(response.json())
@api_view(['GET'])
def get_text(request):
    data = json.loads(request.body)
    data_text = '{"query": {"simple_query_string": {"query": "'+ data["text"] +'"}}}'
    response = requests.get("http://localhost:9200/users/_search", data=data_text.encode('utf-8'),headers={"Content-Type":"application/json"})
    return Response(response.json())
