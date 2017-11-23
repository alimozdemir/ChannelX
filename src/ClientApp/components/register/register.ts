import Vue from 'vue';
import './register.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';


interface registerModel {
    userName: string,
    firstandlastname:string,
    Email: string,
    password: string,
    confirmPassword:string,
    
}

@Component
export default class RegisterComponent extends Vue {
    model: registerModel = { userName: "",firstandlastname:"",Email:"", password: "",confirmPassword:""}
    async register() {
        this.$validator.validateAll();
        let error=this.$validator.errors.any();
        if(!error){
            let result = await axios.post('/api/Account/Register', this.model);
            if (result.status == 200) {
                let data = result.data as resultModel;
    
                if (data.succeeded) {
                    let m_result=await swal({text:"User registered successfully",icon:"success"});
                    if(m_result){
                        this.$router.push('/login');
                    }
                   
                }
                else
                    swal({ text: data.message, icon: "error" });
            }
            else {
                swal({ text: "Something went wrong", icon: "error" });
            }
        }
        else{
            swal({ text: "Invalid informations.", icon: "error" });
        }
       
    }
}
