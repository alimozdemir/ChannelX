import Vue from 'vue';
import './login.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';
import { UserStore } from '../../stores/userState'

interface loginModel {
    userName: string,
    password: string,
    rememberMe: boolean
}

interface UserData {
    auth: string,
    userId: string
}

@Component
export default class LoginComponent extends Vue {
    model: loginModel = { userName: "", password: "", rememberMe: false }

    async login() {
        let result = await axios.post('/api/Account/Login', this.model);

        if (result.status == 200) {
            let data = result.data as resultModel;

            if (data.succeeded) {
                var userData = data.data as UserData;
                UserStore.commitAuthKey(this.$store, userData.auth);
                UserStore.commitUserId(this.$store, userData.userId);
                console.log(userData)
                console.log(UserStore.readAuthKey(this.$store))
                this.$router.push('/');
            }
            else
                swal({ text: data.message, icon: "error" });
        }
        else {
            swal({ text: "something went wrong", icon: "error" });
        }
    }
    
}
