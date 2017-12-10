import Vue from 'vue';
import './forgotpassword.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';

interface forgotPasswordModel {
    email:string,
}

@Component
export default class forgotPasswordComponent extends Vue {
    model: forgotPasswordModel = {email: ""}
    async forgotpassword() {
        let result = await axios.post('/api/Account/Forgotpassword', this.model);
        if (result.status == 200) {
            let data = result.data as resultModel;
            if (data.succeeded) {
                swal({ text: data.message, icon: "success" });
            }
            else
                swal({ text: data.message, icon: "error" });
        }
        else {
            swal({ text: "something went wrong", icon: "error" });
        }
    }
    
}
