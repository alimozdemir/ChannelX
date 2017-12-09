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
const defaultModel: createModel = { title: "", isPrivate: true, password: "", endAtHours: 2 };

@Component
export default class ChannelCreateComponent extends Vue {
    model: createModel = Object.assign({}, defaultModel); //shallow copy

    async submit() {
        this.$validator.validateAll();
        let error = this.$validator.errors.any();

        if (!error) {
            let result = await axios.post('/api/Channel/Create', this.model);

            if (result.status == 200) {
                let response = result.data as resultModel;

                if (response.succeeded) {
                    let data = response.data as string;

                    swal(
                        {
                            title: response.message,
                            icon: "success",
                            content: {
                                element: 'input',
                                attributes: {
                                    value: data
                                }
                            }
                        });
                }
            }
        }

    }

    reset() {
        this.model = Object.assign({}, defaultModel); //shallow copy
    }
}
