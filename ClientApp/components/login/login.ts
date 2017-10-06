import Vue from 'vue';
import './login.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';

@Component
export default class LoginComponent extends Vue {
    userName : string = "";
    password : string = "";
    rememberMe : boolean = false;

    async login(){
        var result = await axios.post('/Account/Login', { userName : this.userName, password : this.password, rememberMe: this.rememberMe });

        if(result.status == 200 && result.data.succeeded)
        {
            localStorage.setItem('auth', result.data.data);
            this.$router.push('/');
        }
        else
        {
            swal({ text : result.data.message ,  icon: "error" });
        }
    }
}
