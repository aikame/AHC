
async function  send_info(){
    let UserLogin = document.getElementById("UserLoginforUserInfo").value;
    await fetch(`http://localhost:7095/User?UserLogin=${UserLogin}`).then(response=>response.json()).then(data=>alert(JSON.stringify(data)));
}

