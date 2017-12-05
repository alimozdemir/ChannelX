import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import resultModel from '../../models/resultModel';
import swal from 'sweetalert';

interface createModel {
    title: string,
    isPrivate: boolean,
    password: string,
    endAtHours: number
}

// default states of model
const defaultModel : createModel = { title: "", isPrivate: true, password: "", endAtHours: 2 };

@Component
export default class ChannelCreateComponent extends Vue {
    model: createModel = Object.assign({}, defaultModel); //shallow copy
    
    async submit() {
        let result = await axios.post('/api/Channel/Create', this.model);
        
        if(result.status == 200){
            let data = result.data as resultModel;

            if (data.succeeded) {
                swal({ text: data.message, icon: "success" });
            }
        }
    }

    reset() {
        this.model = Object.assign({}, defaultModel); //shallow copy
    }
}
