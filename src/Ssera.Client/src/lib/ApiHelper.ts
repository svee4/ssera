import { PUBLIC_API_DOMAIN } from "$env/static/public";

export namespace ApiHelper {
	export const ApiDomain = PUBLIC_API_DOMAIN;

	export type ProblemDetails = {
		status: number;
		type: string;
        title: string;
		detail: string;
        activityTraceId: string;
        requestTraceId: string;
	}

	export type ValidationProblemDetails = ProblemDetails & {
        errors: Record<string, string[]>
    }
}
