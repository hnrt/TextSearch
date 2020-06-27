package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.service.PreferenceService;

@RestController
public class PreferenceController {

	@Autowired
	private PreferenceService preferenceService;
	
	@RequestMapping(value="/v1/preferences/{name}",method=RequestMethod.GET)
	public ValueResponse getPreference(
			@PathVariable String name) {
		String value = preferenceService.getPreference(name);
		ValueResponse rsp = new ValueResponse();
		rsp.setValue(value);
		return rsp;
	}

	@RequestMapping(value="/v1/preferences",method=RequestMethod.POST)
	public void updatePreference(
			@RequestBody UpdatePreferenceRequest req) {
		preferenceService.updatePreference(req);
	}

	@RequestMapping(value="/v1/preferences/{name}",method=RequestMethod.DELETE)
	public void deletePreference(
			@PathVariable String name) {
		preferenceService.deletePreference(name);
	}

}
