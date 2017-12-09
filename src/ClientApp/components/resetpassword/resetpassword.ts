import Vue from 'vue';
import './resetpassword.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';

interface resetpasswordModel {
    password: string,
    confirmpassword: string,
    key: string
}
@Component
export default class ResetPasswordComponent extends Vue {
    model: resetpasswordModel = { password: "", confirmpassword: "", key: "" }

    async mounted() {
        this.model.key = this.$route.params.hash;
        let result = await axios.post("/api/Account/HashControl", this.model);
        if (result.status == 200) {
            let data = result.data as resultModel;
            if (data.succeeded) {
               
            }
            else
            this.$router.push("/login");
        }
        else {
           this.$router.push("/login");
        }
    }
    async reset() {
        this.$validator.validateAll();
        let error = this.$validator.errors.any();
        if (!error) {
            let result = await axios.post('/api/Account/ResetPassword', this.model);

            if (result.status == 200) {
                let data = result.data as resultModel;

                if (data.succeeded) {
                    swal({ text: data.message, icon: "success" });
                    this.$router.push("/login");
                }
                else
                    swal({ text: data.message, icon: "error" });
            }
            else {
                swal({ text: "something went wrong", icon: "error" });
            }
        }
    }

}
