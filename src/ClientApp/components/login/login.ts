import Vue from 'vue';
import './login.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';

interface loginModel {
    userName: string,
    password: string,
    rememberMe: boolean
}

@Component
export default class LoginComponent extends Vue {
    model: loginModel = { userName: "", password: "", rememberMe: false }

    async login() {
        let result = await axios.post('/api/Account/Login', this.model);

        if (result.status == 200) {
            let data = result.data as resultModel;

            if (data.succeeded) {
                localStorage.setItem('auth', data.data);
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
