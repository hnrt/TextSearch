package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@SpringBootTest
public class PreferenceServiceTests {

	@Mock
	private PreferenceRepository preferenceRepository;

	@InjectMocks
	private PreferenceService preferenceService = new PreferenceServiceImpl();

	@Test
	public void getPreference_successful() {
		String name = "quux";
		String value = "fred";
		PreferenceEntity entity = new PreferenceEntity();
		entity.setName(name);
		entity.setValue(value);
		when(preferenceRepository.findByName(name)).thenReturn(entity);
		ValueResponse rsp = preferenceService.getPreference(name);
		Assertions.assertEquals(value, rsp.getValue());
	}

	@Test
	public void getPreference_notfound() {
		String name = "quux";
		when(preferenceRepository.findByName(name)).thenReturn(null);
		ValueResponse rsp = preferenceService.getPreference(name);
		Assertions.assertEquals(null, rsp.getValue());
	}

	@Test
	public void updatePreference_successful() {
		UpdatePreferenceRequest req = new UpdatePreferenceRequest();
		req.setName("foo");
		req.setValue("bar");
		preferenceService.updatePreference(req);
		verify(preferenceRepository, times(1)).save(any(PreferenceEntity.class));
	}

	@Test
	public void deletePreference_successful() {
		String name = "quux";
		String value = "fred";
		PreferenceEntity entity = new PreferenceEntity();
		entity.setName(name);
		entity.setValue(value);
		when(preferenceRepository.findByName(name)).thenReturn(entity);
		preferenceService.deletePreference(name);
		verify(preferenceRepository, times(1)).delete(entity);
	}

	@Test
	public void deletePreference_notfound() {
		String name = "quux";
		when(preferenceRepository.findByName(name)).thenReturn(null);
		preferenceService.deletePreference(name);
		verify(preferenceRepository, times(0)).delete(any(PreferenceEntity.class));
	}
	
	@Test
	public void isServiceUnavailable_true() {
		String name = "enabled";
		String value = "false";
		PreferenceEntity entity = new PreferenceEntity();
		entity.setName(name);
		entity.setValue(value);
		when(preferenceRepository.findByName(name)).thenReturn(entity);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(true, ret);
	}
	
	@Test
	public void isServiceUnavailable_false() {
		String name = "enabled";
		String value = "true";
		PreferenceEntity entity = new PreferenceEntity();
		entity.setName(name);
		entity.setValue(value);
		when(preferenceRepository.findByName(name)).thenReturn(entity);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
	}
	
	@Test
	public void isServiceUnavailable_invalid() {
		String name = "enabled";
		String value = "xyzzy";
		PreferenceEntity entity = new PreferenceEntity();
		entity.setName(name);
		entity.setValue(value);
		when(preferenceRepository.findByName(name)).thenReturn(entity);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
	}
	
	@Test
	public void isServiceUnavailable_null() {
		String name = "enabled";
		when(preferenceRepository.findByName(name)).thenReturn(null);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
	}

	@Test
	public void setServiceAvailability_successful() {
		preferenceService.setServiceAvailability(true);
		verify(preferenceRepository, times(1)).save(any(PreferenceEntity.class));
	}

}
